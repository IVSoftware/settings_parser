﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace settings_parser
{
    public partial class MainForm : Form
    {
        private readonly SettingsService settingsService = new SettingsService();
        private readonly BindingList<Record> DGVDataSource = new BindingList<Record>();

        public MainForm()
        {
            InitializeComponent();
            CheckBox1.CheckedChanged += CheckBox1_CheckedChanged;
            button1.Click += button1_Click;
            dataGridView1.CurrentCellChanged += DataGridView1_CurrentCellChanged;
            dataGridView1.AllowUserToAddRows = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            {
                CheckBox1.DataBindings.Add(
                    nameof(CheckBox1.Checked),
                    settingsService, 
                    nameof(settingsService.IsTestProperty),
                    false,
                    DataSourceUpdateMode.OnPropertyChanged);

                dataGridView1.DataSource = DGVDataSource;
                DGVDataSource.Add(new Record { Description = "Use settings A", Settings = "SettingsParser.A.json" });
                DGVDataSource.Add(new Record { Description = "Use settings B", Settings = "SettingsParser.B.json" });
                dataGridView1.AutoResizeColumns();

                printDebugMessage();
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        { 
            printDebugMessage();
        }

        private void button1_Click(object sender, EventArgs e) => printDebugMessage();

        private void DataGridView1_CurrentCellChanged(object sender, EventArgs e)
        {
            // Load settings based on selected cell

            if (dataGridView1.CurrentCell != null)
            {
                var settingsFileName = DGVDataSource[dataGridView1.CurrentCell.RowIndex].Settings;
                settingsService.ReadFilfe2(settingsFileName);
            }
        }

        private void printDebugMessage([CallerMemberName]string caller = null)
        {
            Debug.WriteLine($"{caller}: SettingsParser.IsTestProperty: {settingsService.IsTestProperty}");
            Text = $"IsTestProperty: {settingsService.IsTestProperty}";
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class SettingsService : INotifyPropertyChanged
    {
        #region Binding Properties
        [JsonProperty]
        public bool IsTestProperty
        {
            get => _IsTestProperty;
            set
            {
                if (!Equals(_IsTestProperty, value))
                {
                    _IsTestProperty = value;
                    OnPropertyChanged();
                }
            }
        }
        bool _IsTestProperty = false;
        #endregion Binding Properties

        //@"e:\Test\WinFrom\Control\Bindings\01\SettingsParser.A.json";
        string settingsDirectory = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Settings");

        internal void ReadFilfe2(string fileName)
        {
            // Deserialize a temporary copy.
            string fullPath = Path.Combine($"{settingsDirectory}", fileName);
            if (!string.Equals(_currentSettingsFile, fullPath))
            {
                _currentSettingsFile = fullPath;
                var tmp = 
                    JsonConvert
                    .DeserializeObject<SettingsService>(
                        File.ReadAllText(_currentSettingsFile));
                // Get the JSON properties
                foreach (
                    var property in
                    typeof(SettingsService).GetProperties()
                    .Where(property => 
                        property
                        .GetCustomAttributes(typeof(JsonPropertyAttribute), true).Any()))
                {
                    // Copy the property value to SettingsService without
                    property.SetValue(this, property.GetValue(tmp));
                }
            }
        }
        internal void Save()
        {
            if(_currentSettingsFile != null)
            {
                File.WriteAllText(_currentSettingsFile, JsonConvert.SerializeObject(this));
            }
        }

        private string _currentSettingsFile = null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class Record
    {
        public string Description { get; set; }
        public string Settings { get; set; }
    }
}