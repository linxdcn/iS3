using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IS3.SimpleStructureTools.Helper.File
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
    public class PathHelper
    {
        /// <summary>
        /// Read the file path of a file. Return a string of the path
        /// </summary>
        /// <param name="extension">Extension of one file type you want, like .txt, .xls</param>
        /// <returns></returns>
        public static string GetPath(string extension)
        {
            string filter = extension + " file(*" + extension + ")|*" + extension;
            filter += "|All file(*.*)|*.*";

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Opem";
            ofd.Filter = filter;
            if (ofd.ShowDialog() == DialogResult.OK)
                return ofd.FileName;
            else
                return "";
        }

        /// <summary>
        /// Save the file
        /// </summary>
        /// <param name="extension"></param>
        public static string GetSaveFilePath(string extension)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = extension + " files(*" + extension + ")|*" + extension + "|All files(*.*)|*.*";
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog1.FileName.ToString();
            }
            else
                return "";
        }
    }
}
