﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;
using DAL;
namespace BL
{
    public class MyBl : IBL
    {
        private DAL.Idal MyDal = FactoryDal.getDal();
        
        private int NumOfTestsByDays(Tester tester, DateTime time)
        {
            return (TestsCollection()).Count(delegate (Test tst) { if (tester.Id == tst.IdTester && DatesAreInTheSameWeek(time, tst.TestDay)) return true; return false; });        
        }
        private bool IsHeFree(Tester item, DateTime time)
        {
            if ((int)time.DayOfWeek > Configuration.THURSDAY || (time.Hour < Configuration.MIN_HOUR || time.Hour > Configuration.MAX_HOUR))
                return false;
            if (!item.WorkTable[time.Hour, (int)time.DayOfWeek])
                return false;
            if (item.MaxTests <=NumOfTestsByDays(item,time))
                return false;
            if (!TestsCollection().TrueForAll(T => (T.IdTester != item.Id) || (T.TestDay != time)))
                return false;
            return true;
        }


        private Test LastTest(Trainee trainee)
        {
            var tests = AllTestsBy(T => T.IdTrainee == trainee.Id);
            Test temp = tests.First<Test>();
            foreach (Test item in tests)
            {
                if (temp.TestDay < item.TestDay)
                {
                    temp = item;
                }
            }
            return temp;
        }
        private bool DatesAreInTheSameWeek(DateTime date1, DateTime date2)
        {
            var cal = System.Globalization.DateTimeFormatInfo.CurrentInfo.Calendar;
            var d1 = date1.Date.AddDays(-1 * (int)cal.GetDayOfWeek(date1));
            var d2 = date2.Date.AddDays(-1 * (int)cal.GetDayOfWeek(date2));

            return d1 == d2;
        }

        public void AddTest(Test test)
        {
            if (test.TestDay <= DateTime.Now)
            {
                throw new Exception("Wrong date!");
            }
            IEnumerable<Tester> testers = TestersCollection();
            IEnumerable<Test> tests = TestsCollection();
            
            var StudentTests = AllTestsBy(T => T.IdTrainee==test.IdTrainee);
            var Trainee = from item in TraineesCollection() where (test.IdTrainee == item.Id) select item;
            if (Trainee==null)
            {
                throw new Exception("this trainee doesnt exist");
            }
            Trainee trainee = Trainee.First();
            if(IsAllowed(trainee))
                throw new Exception("the trainee is already past the test on this type of car");

            Test PreTest = LastTest(trainee);
            
            if ((test.TestDay - PreTest.TestDay).Days < Configuration.RANGE_BETWEEN_TESTS)
                throw new Exception("it's too early to take a new test");
            
            if (trainee.DLessonPast < Configuration.MIN_NUMBER_OF_LESSONS)
                throw new Exception("You need to do "+(Configuration.MIN_NUMBER_OF_LESSONS - trainee.DLessonPast)+" lessons");

            // Leaving only the testers who work on that date
            testers = IsFree(test.TestDay);
            //Leaving only the testers who test on the same type of car.
            testers = from item in testers 
                      let type=test.TypeOfCar 
                      where (item.TypeOfCar == type)
                      select item;
            if (testers==null)
            {
                throw new Exception("there isn't any tester on this date");
            }
            test.IdTester = testers.First().Id;
            MyDal.AddTest(test);
        }

        public void AddTester(Tester tester)
        {
            if((TestersCollection()).Exists(T =>T.Id==tester.Id ))
                throw new Exception("this tester exist");
            if (tester.Age < Configuration.MIN_AGE_TESTER)
            {
                throw new Exception("You are too younger");
            }
            
            MyDal.AddTester(tester);
        }
        public void AddTrainee(Trainee trainee)
        {

            if ((TraineesCollection()).Exists(T => T.Id == trainee.Id))
                throw new Exception("this trainee exist");
            if (trainee.Age < Configuration.MIN_AGE_TRAINEE)
            {
                throw new Exception("You are too younger");
            }
            MyDal.AddTrainee(trainee);
        }

        public void DeleteTester(Tester tester)
        {
            if (!TestersCollection().Exists(T => tester.Id == T.Id))
            {
                throw new Exception("there isn't such tester");
            }
            MyDal.DeleteTester(tester);
        }
        public void DeleteTrainee(Trainee trainee)
        {
            if (!TraineesCollection().Exists(T => trainee.Id == T.Id))
            {
                throw new Exception("there isn't such tester");
            }
            MyDal.DeleteTrainee(trainee);
        }

        public List<Tester> TestersCollection()
        {
            return MyDal.TestersCollection();
        }
        public List<Test> TestsCollection()
        {
            return MyDal.TestsCollection();        
        }
        public List<Trainee> TraineesCollection()
        {
            return MyDal.TraineesCollection();        
        }


        public void Update(Test test)
        {
            if (test.Grade == null)
            {
                throw new Exception("you forget to fill the grade");
            }
            MyDal.Update(test);
        }
        public void UpdateTester(Tester tester)
        {

            if (!TestersCollection().Exists(T => tester.Id == T.Id))
            {
                throw new Exception("there isn't such tester");
            }
            MyDal.UpdateTester(tester);
        }
        public void UpdateTrainee(Trainee trainee)
        {
            if (!TraineesCollection().Exists(T => trainee.Id == T.Id))
            {
                throw new Exception("there isn't such tester");
            }
            MyDal.UpdateTrainee(trainee);
        }
        //the functions
        public IEnumerable<Tester> DistanseFromAdress(Address adress)
        {
            Random r = new Random();
            return from item in TestersCollection() where (r.Next(Configuration.DISTANCE+10)<Configuration.DISTANCE) select item;
        }
        public IEnumerable<Tester> IsFree(DateTime time)
        {
            return from item in TestersCollection() where (IsHeFree(item,time)) select item;
        }
        public IEnumerable<Test> AllTestsBy(Predicate<Test> func)
        {
            return from item in TestsCollection() where (func(item)) select item;
        }
        public int NumOfTraineesTests(Trainee trainee)
        {
            return AllTestsBy(T=>T.IdTrainee==trainee.Id).Count<Test>();
        }
        public bool IsAllowed(Trainee trainee)
        {
            var tests = from item in TestsCollection()
                        let type = trainee.TypeOfCar
                        let id = trainee.Id
                        where (item.IdTrainee == id && item.TypeOfCar == type)
                        orderby item.TestDay descending
                        select item;
            return (tests.First().Grade == Grade.pass);
        }
        public List<Test> ListByDay()
        {
            return new List<Test>(from item in TestsCollection() where(item.TestDay>=DateTime.Now) orderby item.TestDay   select item);
        }
        //...
        public static bool IdCheck(string id)
        {
            if (id == null)
                return false;

            int tmp, count = 0;

            if (!(int.TryParse(id, out tmp)) || id.Length != 9)
                return false;

            int[] id_12_digits = { 1, 2, 1, 2, 1, 2, 1, 2, 1 };
            id = id.PadLeft(9, '0');

            for (int i = 0; i < 9; i++)
            {
                int num = Int32.Parse(id.Substring(i, 1)) * id_12_digits[i];

                if (num > 9)
                    num = (num / 10) + (num % 10);

                count += num;
            }

            return (count % 10 == 0);
        }
        public static void CheckTrainee(Trainee trainee)
        {
            if (!IdCheck(trainee.Id))
                throw new Exception("the ID of the trainee isn't good");

            if (!trainee.PrivateName.All(Char.IsLetter))
                throw new Exception("the Private Name of the trainee isn't good");

            if (!trainee.FamilyName.All(Char.IsLetter))
                throw new Exception("the Family Name of the trainee isn't good");

            if (!trainee.DrivingSchool.All(Char.IsLetter))
                throw new Exception("the Driving School Name isn't good");

            if (!trainee.DrivingTeacher.All(Char.IsLetter))
                throw new Exception("the Driving Teacher Name isn't good");

            if (trainee.Phone.Length!=10 || !trainee.Phone.All(Char.IsDigit))
                throw new Exception("the Phone Number of the trainee isn't good");

            if (trainee.DLessonPast < 0)
                throw new Exception("the number of lesson cant be a negative number");
            if (!trainee.Address.Street.All(char.IsLetter))
                throw new Exception("Wrong  street name!");

            if (!trainee.Address.City.All(char.IsLetter))
                throw new Exception("Wrong city name!");
        }
        public static void CheckTester(Tester tester)
        {
            if (!IdCheck(tester.Id))
                throw new Exception("the ID of the trainee isn't good");

            if (!tester.PrivateName.All(Char.IsLetter))
                throw new Exception("the Private Name of the tester isn't good");

            if (!tester.FamilyName.All(Char.IsLetter))
                throw new Exception("the Family Name of the tester isn't good");

            if (tester.Phone.Length != 10 || !tester.Phone.All(Char.IsDigit))
                throw new Exception("the Phone Number of the tester isn't good");

            if (tester.MaxTests < 0)
                throw new Exception("the maximum of tests in a week cant be negative number");

            if (tester.MaxRange < 0)
                throw new Exception("the Max Range cant be  a negative number");

            if (tester.Years < 0)
                throw new Exception("the Years of experience cant be  a negative number");

            if (!tester.Address.Street.All(char.IsLetter))
                throw new Exception("Wrong  street name!");
            
            if (!tester.Address.City.All(char.IsLetter))
                throw new Exception("Wrong city name!");
        }
        public static void CheckTest(Test test)
        {
            if (test.TestTime.All(char.IsLetter))
                throw new Exception("wrong test time");
            if (test.TestTime != test.TestDay.Day + "/" + test.TestDay.Month + "/" + test.TestDay.Year)
                 throw new Exception("the dates arent same");
            if (!IdCheck(test.IdTester))
                 throw new Exception("Wrong id tester!");
            if (!IdCheck(test.IdTrainee))
                 throw new Exception("Wrong id trainee!");
            if (!test.TestAddress.Street.All(char.IsLetter))
                throw new Exception("Wrong street name!");
            
            if (!test.TestAddress.City.All(char.IsLetter))
                 throw new Exception("Wrong city name!");
            foreach (Criterion item in test.Criterions)
            {
                if (!item.name.All(char.IsLetter))
                {
                    throw new Exception("wrong Criterion "+item.name+"!");
                }
            }
        }
    } 
}
