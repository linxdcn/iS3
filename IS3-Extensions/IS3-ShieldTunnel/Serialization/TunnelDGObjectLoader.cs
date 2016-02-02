using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.Core;
using IS3.Core.Serialization;

namespace IS3.ShieldTunnel.Serialization
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

    public class TunnelDGObjectLoader
    {
        protected TunnelDbDataLoader _dbLoader;

        public TunnelDGObjectLoader(DbContext dbContext)
        {
            _dbLoader = new TunnelDbDataLoader(dbContext);
        }

        public bool LoadTunnels(DGObjects objs)
        {
            DGObjectsDefinition def = objs.definition;
            if (def == null)
                return false;
            bool success = _dbLoader.ReadTunnels(objs,
                def.TableNameSQL, def.ConditionSQL, def.OrderSQL);
            return success;
        }

        public bool LoadAxes(DGObjects objs)
        {
            DGObjectsDefinition def = objs.definition;
            if (def == null)
                return false;
            bool success = _dbLoader.ReadTunnelAxes(objs,
                def.TableNameSQL, def.ConditionSQL, def.OrderSQL);
            return success;
        }

        public bool LoadSLType(DGObjects objs)
        {
            DGObjectsDefinition def = objs.definition;
            if (def == null)
                return false;
            bool success = _dbLoader.ReadSLType(objs,
                def.TableNameSQL, def.ConditionSQL, def.OrderSQL);
            return success;
        }
    }
}
