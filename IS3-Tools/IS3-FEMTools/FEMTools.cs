using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.Core;

namespace IS3.FEMTools
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
    public class FEMTools:Tools
    {
        public override string name() { return "iS3.FEMTools"; }
        public override string provider() { return "Tongji iS3 team"; }
        public override string version() { return "1.0"; }

        List<ToolTreeItem> items;
        public override IEnumerable<ToolTreeItem> treeItems()
        {
            return items;
        }

        #region Windows menber and initialized
        TestWindow testWindow;
        public void testDemo() { Init(testWindow); }
        #endregion

        public void Init<T>(T window) where T : System.Windows.Window, new()
        {
            if (window != null)
            {
                window.Show();
                return;
            }

            window = new T();
            window.Closed += (o, args) =>
            {
                window = null;
            };
            window.Show();
        }

        public FEMTools()
        {
            items = new List<ToolTreeItem>();
            ToolTreeItem item = new ToolTreeItem("FEMTools|Demo", "Test", testDemo);
            items.Add(item);
        }
    }
}
