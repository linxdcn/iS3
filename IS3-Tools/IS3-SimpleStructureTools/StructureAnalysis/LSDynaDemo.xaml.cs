using System;
using System.IO;
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
using System.Windows.Shapes;
using System.Xml;

using IS3.Core;
using IS3.SimpleStructureTools.Helper.File;

namespace IS3.SimpleStructureTools.StructureAnalysis
{
    #region Copyright Notice
    //************************  Notice  **********************************
    //** This file is part of iS3
    //**
    //** Copyright (c) 2015 Tongji University iS3 Team. All rights reserved.
    //**
    //** This library is free software; you can redistribute it and/or
    //** modify it under the terms of the GNU Lesser General Public
    //** License as published by the Free Software Foundation; either
    //** version 3 of the License, or (at your option) any later version.
    //**
    //** This library is distributed in the hope that it will be useful,
    //** but WITHOUT ANY WARRANTY; without even the implied warranty of
    //** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    //** Lesser General Public License for more details.
    //**
    //** In addition, as a special exception,  that plugins developed for iS3,
    //** are allowed to remain closed sourced and can be distributed under any license .
    //** These rights are included in the file LGPL_EXCEPTION.txt in this package.
    //**
    //**************************************************************************
    #endregion

    /// <summary>
    /// Interaction logic for LSDynaDemo.xaml
    /// </summary>
    public partial class LSDynaDemo : Window
    {
        public LSDynaDemo()
        {
            InitializeComponent();

            //load the ansys path
            string savePath = Runtime.rootPath + "//Conf//ansysPath.xml";
            string path = "";
            if (File.Exists(savePath))
            {
                StreamReader sr = new System.IO.StreamReader(savePath);
                XmlTextReader r = new XmlTextReader(sr);
                while (r.Read())
                {
                    if (r.NodeType == XmlNodeType.Element)
                        if (r.Name.ToLower().Equals("path"))
                            path = r.GetAttribute("content");
                }
            }
            TB_Path.Text = path;
        }

        private void Path_Click(object sender, RoutedEventArgs e)
        {
            string ansysPath = PathHelper.GetPath(".exe");
            TB_Path.Text = ansysPath;

            if(ansysPath != "")
            {
                string savePath = Runtime.rootPath + "//Conf//ansysPath.xml";
                FileStream fs;
                if (File.Exists(savePath))
                    fs = new FileStream(savePath, FileMode.Open);
                else
                    fs = new FileStream(savePath, FileMode.Create);
                XmlTextWriter w = new XmlTextWriter(fs, Encoding.UTF8);
                w.WriteStartDocument();
                w.WriteStartElement("path");
                w.WriteAttributeString("content", ansysPath);
                w.WriteEndElement();
                w.Flush();
                fs.Close();
            }     
        }
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            string inputPath = PathHelper.GetPath(".txt");
            StreamReader reader = new System.IO.StreamReader(inputPath);
            string result = "";
            string ss;
            while ((ss = reader.ReadLine()) != null)
                result += ss + "\r\n";
            TB_Input.Text = result;
            TB_InputPath.Text = inputPath;
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            string ansysprogram, inputfile, outputfile;
            ansysprogram = TB_Path.Text;
            inputfile = TB_InputPath.Text;
            outputfile = "D:/LSDynaDemo/output.txt";
            proc.StartInfo.FileName = ansysprogram;
            proc.StartInfo.Arguments = "-b -p ansysds -i " + inputfile + " -o " + outputfile;
            proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            proc.StartInfo.WorkingDirectory = "D:/LSDynaDemo/Result";
            proc.Start();
            proc.WaitForExit();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
