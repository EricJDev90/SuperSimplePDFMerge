﻿using System.IO;
using System.Windows;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PdfCombiner
{
    public partial class MainWindow : Window
    {
        public List<string> FilePaths = new();
        public string OutputFilePath = "";
        public string OutputFileName = "CombinedPDF";


        /**************************************
         * Only one more thing to add: drag and drop list sorting
         * *********************************************/

        public MainWindow()
        {
            InitializeComponent();

            StatusBarStatus.Content = "Ready";
        }

        private void Combine_PDFs()
        {
            Document document = new();

            if (System.String.IsNullOrEmpty(OutputFilePath))
            {
                StatusBarStatus.Content = "Please choose an Output Path";
                return;
            }

            if (FilePaths.Count < 2)
            {
                StatusBarStatus.Content = "You need to add at least 2 pdfs to combine!";
                return;
            }

            if (File.Exists($"{OutputFilePath}/{OutputFileName}.pdf")) {
                StatusBarStatus.Content = "Output file already exits, choose a new path or name";
                return;
            }

            foreach (string path in FilePaths)
            {
                if (!File.Exists(path))
                {
                    StatusBarStatus.Content = "One or more files cannot be found, please try again";
                    return;
                }
            }

            PdfCopy writer = new(document, new FileStream($"{OutputFilePath}/{OutputFileName}.pdf", FileMode.Create));
            if (writer == null)
            {
                return;
            }

            document.Open();
            foreach (string filename in FilePaths)
            {
                try
                {
                    PdfReader reader = new(filename);
                    reader.ConsolidateNamedDestinations();
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        PdfImportedPage page = writer.GetImportedPage(reader, i);
                        writer.AddPage(page);
                    }
                    reader.Close();
                } catch {
                    StatusBarStatus.Content = "Error combining files, please try again";
                    return;
                }
            }
            writer.Close();
            document.Close();
            StatusBarStatus.Content = "Successfully combined PDFs!";
        }


        #region Button Click Handlers
        private void CombineButton_Click(object sender, RoutedEventArgs e)
        {
            Combine_PDFs();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //Set up Open File Dialog box
            int filesAdded = 0;
            OpenFileDialog openFileDialog = new()
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = true
            };

            //If files selected and true chosen
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string fileName in openFileDialog.FileNames)
                {
                    FilePaths.Add(fileName);
                    filesAdded++;
                }
                ListOfPDFs.ItemsSource = null;
                ListOfPDFs.ItemsSource = FilePaths;

                StatusBarStatus.Content = $"{filesAdded} files added" ;
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        { 
            if (ListOfPDFs.SelectedItems.Count == 0)
            {
                StatusBarStatus.Content = "You must select a file or files to remove";
                return;
            }
            
            int filesRemoved = 0;
            //Remove file paths and names
            for(int i = 0; i < ListOfPDFs.SelectedItems.Count; i++)
            {
                var selectedItem = ListOfPDFs.SelectedItems[i];
                
                if (selectedItem != null)
                {
                    selectedItem.ToString();
                    FilePaths.Remove((string)selectedItem);
                    filesRemoved++;
                }
                
            }

            //Reset view
            ListOfPDFs.ItemsSource = null;
            ListOfPDFs.ItemsSource = FilePaths;
            StatusBarStatus.Content = $"{filesRemoved} files removed";
        }

        private void EditPathButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutputFilePath= folderBrowserDialog.SelectedPath;
                OutputPath.Text = OutputFilePath;
                StatusBarStatus.Content = "Ouput Path Set";
            }
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Created by Eric Jansen ©2023", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        private void OutputFileNameText_Change(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            OutputFileName = OutputFileNameTextBox.Text;
        }
    }
}