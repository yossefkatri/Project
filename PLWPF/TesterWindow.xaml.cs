﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BE;
using BL;
namespace PLWPF
{
    /// <summary>
    /// Interaction logic for TesterWindow.xaml
    /// </summary>
    public partial class TesterWindow : Window
    {
        IBL bl;
        int index = 0;
        public Tester tester { get; set; }
        public List<Test> tests { get; set; }
        public TesterWindow(Tester tester)
        {
            bl = BL.FactoryBL.getBl();
            InitializeComponent();
            this.typeOfCarComboBox.ItemsSource = Enum.GetValues(typeof(BE.Car));
            this.gradecheckbox.ItemsSource = Enum.GetValues(typeof(BE.Grade));
            this.gradecheckbox.FontWeight = FontWeights.DemiBold;
            this.gradecheckbox.FontSize = 20;
            this.tester = tester;
            this.DataContext = tester;
            Address address = tester.Address;
            this.City.Text = address.City;
            this.Street.Text = address.Street;
            this.NumOfHome.Text = address.NumOfHome;
            tests = new List<Test>(bl.AllTestsBy(T=>T.IdTester==tester.Id));

        }

        private void WorkTableButton_Click(object sender, RoutedEventArgs e)
        {
            WorkTable workTableWin = new WorkTable(tester.WorkTable);
            workTableWin.ShowDialog();
            tester.WorkTable = workTableWin.workTable;
            Button b1 = sender as Button;
            b1.Content = "!סיימת";
        }

        private void Sign_Click(object sender, RoutedEventArgs e)
        {
            tester.Address = new Address(this.City.Text,this.Street.Text,this.NumOfHome.Text);
            try
            {
                bl.UpdateTester(tester);
            }
            catch(Exception exp)
            {
                this.WarningBox.Content = exp.Message;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                warnningBox delete = new warnningBox(tester);
                delete.ShowDialog();
                if (delete.IsDelete)
                {
                bl.DeleteTester(tester);
                this.Close();
                }
            }
            catch (Exception exp)
            {
                this.DeleteWarningBox.Content = exp.Message;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                tester.Code= this.Code.Password;
                bl.UpdateTester(tester);
            }
            catch (Exception exp)
            {

            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Criterion_Click(object sender, RoutedEventArgs e)
        {
            Label name = new Label();
            name.Content = ":שם";
            name.FontWeight = FontWeights.DemiBold;
            TextBox name1 = new TextBox();
            Label grade = new Label();
            grade.Content = ":ציון";
            grade.FontWeight =FontWeights.DemiBold;
            ComboBox grade1 = new ComboBox();
            grade1.ItemsSource = Enum.GetValues(typeof(BE.Grade));
            grade1.FontWeight = FontWeights.DemiBold;
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetColumn(name,1);
            Grid.SetRow(name, 0);
            Grid.SetColumn(name1, 0);
            Grid.SetRow(name1, 0);
            Grid.SetColumn(grade, 1);
            Grid.SetRow(grade, 1);
            Grid.SetColumn(grade1, 0);
            Grid.SetRow(grade1, 1);
            grid.Children.Add(name);
            grid.Children.Add(name1);
            grid.Children.Add(grade);
            grid.Children.Add(grade1);
            panel.Children.Add(grid);
        }
        private void Enter_Click(object sender, EventArgs e)
        {
            //להכניס את הנתונים לתוך המבחן הנבחר
        }
    }
}
