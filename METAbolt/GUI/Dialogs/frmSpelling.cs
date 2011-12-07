﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenMetaverse;
using NHunspell;
using System.Text.RegularExpressions;  

namespace METAbolt
{
    public partial class frmSpelling : Form
    {
        private METAboltInstance instance;
        private Hunspell hunspell = new Hunspell();   //("en_us.aff", "en_us.dic");
        private string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\METAbolt\\Spelling\\";
        //private string words = string.Empty;
        private int start = 0;
        private int indexOfSearchText = 0;
        private string[] swords;
        //private int swordind = -1;
        private string currentword = string.Empty;
        private List<string> mistakes = new List<string>();
        private ChatType ctype;
        private string afffile = string.Empty;
        private string dicfile = string.Empty;
        private string dic = string.Empty;

        public frmSpelling(METAboltInstance instance, string sentence, string[] swords, ChatType type)
        {
            InitializeComponent();

            this.instance = instance;

            afffile = "en_GB.aff";
            dicfile = "en_GB.dic";

            string[] idic = dicfile.Split('.');
            dic = dir + idic[0];

            if (!System.IO.File.Exists(dic + ".csv"))
            {
                System.IO.File.Create(dic + ".csv");
            }

            hunspell.Load(dir + afffile, dir + dicfile);   //("en_us.aff", "en_us.dic");
            ReadWords();

            //words = sentence;
            richTextBox1.Text = sentence;
            this.swords = swords;
            this.ctype = type;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            instance.TabConsole.chatConsole._textBox = richTextBox1.Text;

            this.Close(); 
        }

        private void frmSpelling_Load(object sender, EventArgs e)
        {
            CheckSpellings();
        }

        private void CheckSpellings()
        {
            mistakes.Clear();
            listBox1.Items.Clear();  
 
            foreach (string word in swords)
            {
                string cword = Regex.Replace(word, @"[^a-zA-Z0-9]", "");

                bool correct = hunspell.Spell(cword);

                if (!correct)
                {
                    InHighligtWord(cword);

                    mistakes.Add(cword);
                }
            }

            if (mistakes.Count > 0)
            {
                start = 0;
                indexOfSearchText = 0;

                currentword = mistakes[0];

                HighligtWord(currentword);

                List<string> suggestions = hunspell.Suggest(currentword);

                foreach (String entry in suggestions)
                {
                    listBox1.Items.Add(entry);
                }
            }
        }

        private void ContSearch()
        {
            listBox1.Items.Clear();
            button4.Enabled = false; 

            if (currentword.Contains(currentword))
            {
                mistakes.Remove(currentword);
            }

            if (mistakes.Count < 1)
            {
                instance.TabConsole.chatConsole._textBox = richTextBox1.Text;
                currentword = string.Empty;

                this.Close();
            }
            else
            {
                currentword = mistakes[0];

                HighligtWord(mistakes[0]);

                List<string> suggestions = hunspell.Suggest(mistakes[0]);

                foreach (String entry in suggestions)
                {
                    listBox1.Items.Add(entry);
                }
            }
        }

        private void HighligtWord(string word)
        {
            int startindex = 0;

            startindex = FindText(word.Trim(), start, richTextBox1.Text.Length);

            if (startindex >= 0)
            {
                // Set the highlight color as red
                richTextBox1.SelectionColor = Color.Red;
                richTextBox1.SelectionBackColor = Color.Yellow;   
                // Find the end index. End Index = number of characters in textbox
                int endindex = word.Length;
                // Highlight the search string
                richTextBox1.Select(startindex, endindex);
                // mark the start position after the position of
                // last search string
                start = startindex + endindex;
            }
        }

        private void ReplaceWord(string word)
        {
            if (start >= 0)
            {
                int endindex = currentword.Length;

                richTextBox1.SelectionColor = Color.Black;
                richTextBox1.SelectionBackColor = Color.White;

                richTextBox1.SelectionStart = start;
                richTextBox1.SelectionLength = endindex;

                richTextBox1.SelectedText = word;  

                endindex = word.Length;

                // last search string
                start = start + endindex;

                ContSearch();
            }
        }

        private void InHighligtWord(string word)
        {
            int startindex = 0;

            startindex = FindText(word.Trim(), start, richTextBox1.Text.Length);

            if (startindex >= 0)
            {
                // Set the highlight color as red
                richTextBox1.SelectionColor = Color.Red;
                //richTextBox1.SelectionBackColor = Color.Yellow;
                // Find the end index. End Index = number of characters in textbox
                int endindex = word.Length;
                // Highlight the search string
                richTextBox1.Select(startindex, endindex);
                // mark the start position after the position of
                // last search string
                start = startindex + endindex;
            }
        }

        public int FindText(string txtToSearch, int searchStart, int searchEnd)
        {
            // Unselect the previously searched string
            if (searchStart > 0 && searchEnd > 0 && indexOfSearchText >= 0)
            {
                //richTextBox1.Undo();
                richTextBox1.ForeColor = Color.Black;
                richTextBox1.SelectionBackColor = Color.White; 
            }

            // Set the return value to -1 by default.
            int retVal = -1;

            // A valid starting index should be specified.
            // if indexOfSearchText = -1, the end of search
            if (searchStart >= 0 && indexOfSearchText >= 0)
            {
                // A valid ending index
                if (searchEnd > searchStart || searchEnd == -1)
                {
                    // Find the position of search string in RichTextBox
                    indexOfSearchText = richTextBox1.Find(txtToSearch, searchStart, searchEnd, RichTextBoxFinds.None);
                    // Determine whether the text was found in richTextBox1.
                    if (indexOfSearchText != -1)
                    {
                        // Return the index to the specified search text.
                        retVal = indexOfSearchText;
                    }
                }
            }
            return retVal;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            instance.TabConsole.chatConsole._textBox = richTextBox1.Text;

            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ContSearch();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(currentword))
            {
                hunspell.Add(currentword);
                AddWord(currentword);
            }

            ContSearch();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button4.Enabled = (listBox1.SelectedIndex != -1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                start = start - currentword.Length;
                ReplaceWord(listBox1.SelectedItem.ToString());

                listBox1.SelectedIndex = -1;
                button4.Enabled = false; 
            }
        }

        private void frmSpelling_FormClosing(object sender, FormClosingEventArgs e)
        {
            instance.TabConsole.chatConsole.SendChat(richTextBox1.Text, ctype);
        }

        private void AddWord(string aword)
        {
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\METAbolt\\Spelling\\";
            string dicfile = "en_GB.csv";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(dir + dicfile, true))
            {
                file.WriteLine(aword + ",");
            }

            CheckSpellings();
        }

        private void ReadWords()
        {
            using (CsvFileReader reader = new CsvFileReader(dic + ".csv"))
            {
                CsvRow row = new CsvRow();

                while (reader.ReadRow(row))
                {
                    foreach (string s in row)
                    {
                        hunspell.Add(s);
                    }
                }

                reader.Dispose();  
            }
        }
    }
}