﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using WPFProjekt.Models;
using WPFProjekt.Services.Interfaces;
using WPFProjekt.Services;
using Microsoft.Win32;
using IronPdf;
using static Azure.Core.HttpHeader;

namespace WPFProjekt
{
    /// <summary>
    /// Logika interakcji dla klasy PdfExportFormWindow.xaml
    /// </summary>
    public partial class PdfExportFormWindow : Window
    {

        private readonly INoteService _noteService = new NotesService();
        private readonly ICategoryService _categoryService = new CategoryService();

        public ObservableCollection<Note> NotesList { get; set; } = new ObservableCollection<Note>();
        public ObservableCollection<Category> Categories;
        public ObservableCollection<Category> CategoriesToPrintList = new ObservableCollection<Category>();

        public PdfExportFormWindow()
        {
            InitializeComponent();

            GetAllCategories();
            GetAllNotes();
        }

        private async void GetAllCategories()
        {
            var tmp = await _categoryService.GetAllAsync();
            Categories = new ObservableCollection<Category>(tmp);

            CategoriesComboBox.ItemsSource = Categories;
        }

        public async void GetAllNotes()
        {
            var tmp = await _noteService.GetAllAsync();

            NotesList = new ObservableCollection<Note>(tmp);
        }

        private void AllCB_Checked(object sender, RoutedEventArgs e)
        {
            CategoriesComboBox.IsEnabled = false;

        }

        private void AllCB_Unchecked(object sender, RoutedEventArgs e)
        {
            CategoriesComboBox.IsEnabled = true;
        }


        public string getFilePath()
        {
            
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            
            saveFileDialog1.InitialDirectory = @"D:\";           
            saveFileDialog1.Title = "Save the PDF Files";         
            saveFileDialog1.CheckPathExists = true;         
            saveFileDialog1.DefaultExt = ".pdf";            
            saveFileDialog1.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";           
            saveFileDialog1.FilterIndex = 2;            
            saveFileDialog1.RestoreDirectory = true;
            
            if (saveFileDialog1.ShowDialog() == true)
            {               
                return saveFileDialog1.FileName;
            }
            
            return "";
        }

        public async Task<string> getHtml()
        {
            string output = "";
            string sort = "None";
            if (AllCB.IsChecked == true) CategoriesToPrintList = Categories;
            else
            {
                Category cat = CategoriesComboBox.SelectedItem as Category;
                CategoriesToPrintList.Add(cat);
            }

           // CategoriesToPrintList.Add((Category)CategoriesComboBox.SelectedItem);

            if (titleCB.IsChecked == true) output += $"<h1>{titleTextBox.Text}</h1>";

            DateTime dt = DateTime.Now;
            if (DataCB.IsChecked == true) output += dt;
            else if(DiffDateCB.IsChecked == true) output += DatePicker1.SelectedDate.Value.ToShortDateString();




            if (PrioritySortRB.IsChecked == true)
            {               
                sort = "By Priority";
            }
            else if (TitleSortRB.IsChecked == true)
            {             
                sort = "By Title";
            }

            output += $"<br/> Sort: {sort} <br/>";

            foreach (Category cat in CategoriesToPrintList)
            {

                var notes = NotesList.Where(n => n.CategoryId == cat.Id);

                if (NocatTitlePrintRB.IsChecked == true && notes != null) output += "<br/>";
                else output += $"<h3 id=\"title\">{cat.Name}</h3>";

                
                if(PrioritySortRB.IsChecked==true)
                {
                    notes = notes.OrderBy(n => n.Priority);
                    sort = "By Priority";
                }
                else if(TitleSortRB.IsChecked == true)
                {
                    notes = notes.OrderBy(n => n.Title);
                    sort = "By Title";
                }

                foreach (Note note in notes)
                {
                    
                    if (FieldsCB.IsChecked == true)
                    {
                        output += "<div>";
                        output += "<span style='font-size:20px;'> &#9634;</span> &nbsp;";
                        if (titleCB.IsChecked == true)
                            output += $"<b>{note.Title}</b> &nbsp;";
                        if (PriorityCB.IsChecked == true)
                            output += $"Priority: {note.Priority} &nbsp;";
                        if (ContentCB.IsChecked == true)
                            output += $"Content: {note.Content} &nbsp;";
                        if (cdateCB.IsChecked == true)
                            output += $"Create date: {note.createDateTime} &nbsp;";
                        if(edateCB.IsChecked == true)
                            output += $"Update date: {note.updateDateTime} &nbsp;";
                        output += "</div>";
                    }
                    else
                    {
                        output += $"<div> <b>{note.Title}</b> <br/> Priority: {note.Priority} <br/> Content: <br/> {note.Content} <br/> Create date:" +
                            $" {note.createDateTime} <br/> Update date: {note.updateDateTime} <br/>  </div> ";

                        if (note.IsDone) output += "<h5 style=\"color: LightGreen\">Done</h5> <br/>";
                        else output += "<h5 style=\"color: red\">Not done</h5> <br/>";
                    }
                    
                }
            }
            output += "<style>\r\n#title {\r\n\ttext-align: center;\r\n}\r\n</style>";
            return output;
        }

        private void GeneratePdf(object sender, RoutedEventArgs e)
        {
            string filename = getFilePath();

            if (!String.IsNullOrEmpty(filename))
            {
                string html = getHtml().Result;
                var renderer = new ChromePdfRenderer();                
                var pdfDocument = renderer.RenderHtmlAsPdf(html);
                
                pdfDocument.SaveAs(filename);
                
                MessageBox.Show("PDF has been saved Successfully!");
            }
        }

        private void DataCB_Checked(object sender, RoutedEventArgs e)
        {
            DiffDateCB.IsChecked = false;
        }

        private void DataCB_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void DiffDateCB_Checked(object sender, RoutedEventArgs e)
        {
            DataCB.IsChecked = false;
        }

        private void DiffDateCB_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        /*
        private void CategorySelectCB_Checked(object sender, RoutedEventArgs e)
        {
            Category cat = CategoriesComboBox.SelectedItem as Category;
            if(cat is null)
            {
                MessageBox.Show("Wybierz kategorie!!");
            }
            else
            {
                CategoriesToPrintList.Add(cat);
            }
        }

        private void CategorySelectCB_Unchecked(object sender, RoutedEventArgs e)
        {
            Category cat = CategoriesComboBox.SelectedItem as Category;
            if (cat is null)
            {
                MessageBox.Show("Wybierz kategorie!!");
            }
            else
            {
                CategoriesToPrintList.Remove(cat);
            }
        }
        */
    }
}
