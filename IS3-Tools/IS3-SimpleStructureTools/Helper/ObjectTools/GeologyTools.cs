using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.Core;
using IS3.Core.Geometry;
using IS3.Core.Graphics;
using IS3.Geology;
using IS3.SimpleStructureTools.Helper;

namespace IS3.SimpleStructureTools.Helper
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
    public class GeologyTools
    {
        public static SoilProperty GetSoilProperty(string name, double mileage)
        {
            Project prj = Globals.project;
            Domain strDomain = prj.getDomain(DomainType.Geology);

            int stratumSectionID = getStratumSectionID(mileage);
            if (stratumSectionID == 0)
                return null;

            DGObjectsCollection soilPropertys = strDomain.getObjects("SoilProperty");
            List<DGObject> objList = soilPropertys.merge();
            foreach (DGObject obj in objList)
            {
                SoilProperty sp = obj as SoilProperty;

                if (sp.name == name && sp.StratumSectionID == stratumSectionID)
                    return sp;
            }
            return null;
        }

        //return the StratumSectionID
        //0 represent do not find one
        public static int getStratumSectionID(double mileage)
        {
            Project prj = Globals.project;
            Domain strDomain = prj.getDomain(DomainType.Geology);
            DGObjectsCollection straSections = strDomain.getObjects("StratumSection");
            var objList = straSections.merge();
            foreach(DGObject sec in objList)
            {
                StratumSection straSec = sec as StratumSection;
                if (mileage > straSec.StartMileage &&
                    mileage < straSec.EndMileage)
                    return straSec.id;
            }
            return 0;
        }
    }
}
