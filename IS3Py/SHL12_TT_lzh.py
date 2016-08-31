# -*- coding:gb2312 -*-
import is3

from System.Collections.ObjectModel import ObservableCollection
from System.Windows.Media import Colors

def add3dview():
    is3.addView3d('Map3D', 'Test.unity3d')

def addBaseMap():
    is3.mainframe.LoadProject('SHL12_TT_lzh.xml')
    is3.prj = is3.mainframe.prj
    is3.MainframeWrapper.loadDomainPanels()
    
    emap = is3.EngineeringMap('BaseMap',
                              13523000, 3664000, 13525000, 3666000, 0.1)
    emap.LocalTileFileName1 = 'Shanghai_LOD16_CityBlocks.tpk'
    emap.LocalTileFileName2 = 'plan_map_L12.tpk'
    emap.LocalGeoDbFileName = 'plan_map.geodatabase'

    viewWP = is3.MainframeWrapper.addView(emap)
    return viewWP

def addCrossMap():
    emap = is3.EngineeringMap('CRProfileMap',
                               13523000, 3664000, 13525000, 3680000, 0.1)
    emap.LocalTileFileName1 = 'cross_tunnel_profile_map.tpk'
    emap.LocalGeoDbFileName = 'cross_tunnel_profile_map.geodatabase'
    viewWP = is3.MainframeWrapper.addView(emap)
    return viewWP

def addLonMap():
    emap = is3.EngineeringMap('LTProfileMap',
                              13521600, 3662850, 13524300, 3663350, 0.1)
    emap.LocalTileFileName1 = 'Empty.tpk'
    emap.LocalGeoDbFileName = 'longitudinal_tunnel_profile_map.geodatabase'
    viewWP = is3.MainframeWrapper.addView(emap)
    return viewWP

def addBhLayer(viewWP):
    defaultsymbol = is3.SimpleMarkerSymbolDef(Colors.Blue, 10.0, is3.SimpleMarkerStyle.Circle)
    symbol1 = is3.SimpleMarkerSymbolDef(Colors.Green, 10.0, is3.SimpleMarkerStyle.Circle)
    symbol2 = is3.SimpleMarkerSymbolDef(Colors.Red, 10.0, is3.SimpleMarkerStyle.Circle)
    fields = ObservableCollection[str](['BoreholeType'])
    info1 = is3.UniqueValueInfoDef(symbol1, ObservableCollection[object](['取土孔']))
    info2 = is3.UniqueValueInfoDef(symbol2, ObservableCollection[object](['静力触探孔']))
    infos = ObservableCollection[is3.UniqueValueInfoDef]((info1, info2))
    uniquevalue_renderer = is3.UniqueValueRendererDef(defaultsymbol, fields, infos)
    
    layerDef = is3.LayerDef()
    layerDef.Name = 'GEO_BHL'
    layerDef.GeometryType = is3.GeometryType.Point
    #layerDef.Color = Colors.Blue
    #layerDef.MarkerSize = 12
    #layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerDef.RendererDef = uniquevalue_renderer
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    bhLayerWrapper = is3.addGdbLayer(viewWP, layerDef)
    return bhLayerWrapper

def addRinLayer(viewWP):
    layerDef = is3.LayerDef()
    layerDef.Name = 'DES_RIN'
    layerDef.GeometryType = is3.GeometryType.Polygon
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.LightGray
    layerDef.FillStyle = is3.SimpleFillStyle.Solid
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.LabelWhereClause = "[Name] LIKE '%000' OR [Name] LIKE '%500' OR [Name] LIKE '%0001'"
    layerDef.LabelBackgroundColor = Colors.Yellow
    layerDef.LabelFontSize = 10
    rinLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return rinLayerWP

def addTunLayer(viewWP):
    layerDef = is3.LayerDef()
    layerDef.Name = 'DES_TUN'
    layerDef.GeometryType = is3.GeometryType.Polygon
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Blue
    layerDef.FillStyle = is3.SimpleFillStyle.Solid
    tunLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return tunLayerWP

def addStatLayer(viewWP):
    layerDef = is3.LayerDef()
    layerDef.Name = 'DES_STA'
    layerDef.GeometryType = is3.GeometryType.Polyline
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Blue
    layerDef.FillStyle = is3.SimpleFillStyle.Solid
    statLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return statLayerWP

def addAxesLayer(viewWP):
	layerDef = is3.LayerDef()
	layerDef.Name = 'DES_AXL'
	layerDef.GeometryType = is3.GeometryType.Polyline
	layerDef.OutlineColor = Colors.Green
	layerDef.Color = Colors.Green
	layerDef.FillStyle = is3.SimpleFillStyle.Solid
	axesLayerWP = is3.addGdbLayer(viewWP, layerDef)
	return axesLayerWP

def addStrLayer(viewWP):
    layerDef = is3.LayerDef()
    layerDef.Name = 'GEO_STR'
    layerDef.GeometryType = is3.GeometryType.Polygon
    layerDef.OutlineColor = Colors.Black
    layerDef.Color = Colors.LightGray
    layerDef.FillStyle = is3.SimpleFillStyle.Solid
    strLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return strLayerWP

def addStrLayer(viewWP):
    layerDef = is3.LayerDef()
    layerDef.Name = 'GEO_STR'
    layerDef.GeometryType = is3.GeometryType.Polygon
    layerDef.OutlineColor = Colors.Gray
    layerDef.Color = Colors.LightGray
    layerDef.FillStyle = is3.SimpleFillStyle.Solid
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.LabelBackgroundColor = Colors.White
    strLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return strLayerWP

def addGateWaysLayer(viewWP):  #网关
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_GW_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Triangle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerDef.LabelBackgroundColor = Colors.Yellow
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
	
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_GW'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Triangle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return monLayerWP

def addCRQJMonitoringLayer(viewWP):  #横断面中的双倾角支点
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_DIP'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.Color = Colors.Blue
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)
	
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_DPGP'
    layerDef.GeometryType = is3.GeometryType.Polyline
    layerDef.Color = Colors.Cyan
    layerDef.LineWidth = 3
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'  
    layerDef.IsVisible = False
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)

    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_DIP_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.Color = Colors.Blue
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)
	
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_DPGP_TJ'
    layerDef.GeometryType = is3.GeometryType.Polyline
    layerDef.Color = Colors.Cyan
    layerDef.LineWidth = 3
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False  
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)
   
    return layerWrapper

def addCRJFMonitoringLayer(viewWP):  #横断面中的接缝张开支点
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_JF'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerDef.LabelBackgroundColor = Colors.Green
    layerDef.MarkerSize = 12   
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Triangle	
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)
	
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_JFGP'
    layerDef.GeometryType = is3.GeometryType.Polyline
    layerDef.Color = Colors.Cyan
    layerDef.LineWidth = 3
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)
	
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_JF_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerDef.LabelBackgroundColor = Colors.Green
    layerDef.MarkerSize = 12   
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Triangle	
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)
	
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_JFGP_TJ'
    layerDef.GeometryType = is3.GeometryType.Polyline
    layerDef.Color = Colors.Cyan
    layerDef.LineWidth = 3
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)
	
    return layerWrapper
	
def addCRLEAKMonitoringLayer(viewWP):	#横断面中的渗漏水支点
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_LEAK_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerDef.LabelBackgroundColor = Colors.Green
    layerDef.MarkerSize = 12   
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Triangle	
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)	
	
    return layerWrapper
	
def addCRACMonitoringLayer(viewWP):	  #横断面中的加速度传感器支点
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_ACE_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerDef.LabelBackgroundColor = Colors.Green
    layerDef.MarkerSize = 12   
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Triangle	
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)	
	
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_ACGP_TJ'
    layerDef.GeometryType = is3.GeometryType.Polyline
    layerDef.Color = Colors.Cyan
    layerDef.LineWidth = 3
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)
    return layerWrapper
	
def addCRLQJMonitoringLayer(viewWP):	 #横断面中的纵向倾角传感器支点
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_LDIP_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerDef.LabelBackgroundColor = Colors.Green
    layerDef.MarkerSize = 12   
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Triangle	
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)	
	
    return layerWrapper
	
	
def addLPlanQJMonitoringLayer(viewWP):  #纵断面图中的双倾角支点
    layerDef = is3.LayerDef() 
    layerDef.Name = 'MON_DIP'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]' 
    layerDef.IsVisible = False 
    layerDef.LabelBackgroundColor = Colors.Yellow
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
	
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_DIP_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]' 
    layerDef.IsVisible = False 
    layerDef.LabelBackgroundColor = Colors.Yellow
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)			
	
    return monLayerWP
		
def addLPlanJFMonitoringLayer(viewWP):   #纵断面图中的接缝张开支点
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_JF'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Triangle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerDef.LabelBackgroundColor = Colors.Yellow
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
	
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_JF_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Triangle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return monLayerWP

def addLPlanLEAKMonitoringLayer(viewWP):  #纵断面图中的渗漏水传感器
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_LEAK_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False

    layerDef.LabelBackgroundColor = Colors.Yellow
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
	
    return monLayerWP
	
def addLPlanACMonitoringLayer(viewWP):  #纵断面图中的加速度传感器
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_ACE_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False

    layerDef.LabelBackgroundColor = Colors.Yellow
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
	
    return monLayerWP

def addLPlanLQJMonitoringLayer(viewWP):  #纵断面图中的纵向倾角传感器支点
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_LDIP_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerDef.LabelBackgroundColor = Colors.Yellow
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
	
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_LDPGP'
    layerDef.GeometryType = is3.GeometryType.Polyline
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False	
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
	
    return monLayerWP	
	
	
def addPlanQJMonitoringLayer(viewWP):  #平面图中的双倾角支点
    layerDef = is3.LayerDef() 
    layerDef.Name = 'MON_DIP'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]' 
    layerDef.IsVisible = False 
    layerDef.LabelBackgroundColor = Colors.Yellow
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
	
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_DIP_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]' 
    layerDef.IsVisible = False 
    layerDef.LabelBackgroundColor = Colors.Yellow
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)			
	
    return monLayerWP
	
def addPlanJFMonitoringLayer(viewWP):  #平面图中的接缝张开支点
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_JF'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Triangle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerDef.LabelBackgroundColor = Colors.Yellow		
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
	
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_JF_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Triangle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerDef.LabelBackgroundColor = Colors.Yellow		
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
	
    return monLayerWP

def addPlanLEAKMonitoringLayer(viewWP):  #平面图中的渗漏水传感器
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_LEAK_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False

    layerDef.LabelBackgroundColor = Colors.Yellow
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return monLayerWP
	
def addPlanACMonitoringLayer(viewWP):  #平面图中的加速度传感器
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_ACE_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False

    layerDef.LabelBackgroundColor = Colors.Yellow
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return monLayerWP
	
def addPlanLQJMonitoringLayer(viewWP):  #平面图中的纵向倾角传感器支点
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_LDIP_TJ'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerDef.IsVisible = False
    layerDef.LabelBackgroundColor = Colors.Yellow
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
	
    layerDef = is3.LayerDef()
    layerDef.Name = 'MON_LDPGP'
    layerDef.GeometryType = is3.GeometryType.Polyline
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Black
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'	
    layerDef.IsVisible = False
    monLayerWP = is3.addGdbLayer(viewWP, layerDef)
	
    return monLayerWP	

def addLayers(viewWP):
    is3.addGdbLayerLazy(viewWP, 'DES_PIT_WALL', is3.GeometryType.Polyline)
    is3.addGdbLayerLazy(viewWP, 'DES_AXP', is3.GeometryType.Point)
    is3.addGdbLayerLazy(viewWP, 'DES_PAS', is3.GeometryType.Polygon)
    is3.addGdbLayerLazy(viewWP, 'DES_AXL', is3.GeometryType.Polyline)
    is3.addGdbLayerLazy(viewWP, 'DES_TUN', is3.GeometryType.Polygon)
    
    is3.addGdbLayerLazy(viewWP, 'MON_WAT', is3.GeometryType.Point)
    is3.addGdbLayerLazy(viewWP, 'MON_RIN', is3.GeometryType.Point)
    is3.addGdbLayerLazy(viewWP, 'MON_GRO', is3.GeometryType.Point)
    is3.addGdbLayerLazy(viewWP, 'MON_BUI', is3.GeometryType.Point)

def changeRenderer():
    defaultsymbol = is3.graphicsEngine.newSimpleMarkerSymbol(Colors.Red, 12.0, is3.SimpleMarkerStyle.X)
    symbol1 = is3.graphicsEngine.newSimpleMarkerSymbol(Colors.Green, 12.0, is3.SimpleMarkerStyle.X)
    symbol2 = is3.graphicsEngine.newSimpleMarkerSymbol(Colors.Black, 12.0, is3.SimpleMarkerStyle.X)
    fields = ObservableCollection[str](['BoreholeType'])
    info1 = is3.graphicsEngine.newUniqueValueInfo(symbol1, ObservableCollection[object](['取土孔']))
    info2 = is3.graphicsEngine.newUniqueValueInfo(symbol2, ObservableCollection[object](['静力触探孔']))
    infos = ObservableCollection[is3.IUniqueValueInfo]((info1, info2))
    uniquevalue_renderer = is3.graphicsEngine.newUniqueValueRenderer(defaultsymbol, fields, infos)
    bhLayerWP.setRenderer(uniquevalue_renderer)

def addShapefile(name, type, file, start = 0, maxFeatures = 0):
    layerDef = is3.LayerDef()
    layerDef.Name = name
    layerDef.GeometryType = type
    bkgLayerWP = is3.addShpLayer(viewWP, layerDef, file, start, maxFeatures)
    return bkgLayerWP

def test1():
    addShapefile('BKG1', is3.GeometryType.Polyline, 'bkg_lin.shp', 0, 30000)

def test2():
    addShapefile('BKG2', is3.GeometryType.Polyline, 'bkg_lin.shp', 30000, 30000)

def test3():
    addShapefile('BKG3', is3.GeometryType.Polyline, 'bkg_lin.shp', 60000, 30000)

def Load():
    global viewWP1, viewWP2, viewWP3, safe_view

    print("--- Add base map ---")
    viewWP1 = addBaseMap()
    axisLayerWP = addAxesLayer(viewWP1)
    tunLayerWP = addTunLayer(viewWP1)
    rinLayerWP = addRinLayer(viewWP1)
    bhLayerWP = addBhLayer(viewWP1)
    addGateWaysLayer(viewWP1)
    addPlanQJMonitoringLayer(viewWP1)
    addPlanJFMonitoringLayer(viewWP1)
    addPlanLEAKMonitoringLayer(viewWP1)
    addPlanACMonitoringLayer(viewWP1)
    addPlanLQJMonitoringLayer(viewWP1)
	

    print ("--- Add longitudinal profile map ---")
    viewWP2 = addLonMap()
    addStrLayer(viewWP2)
    addRinLayer(viewWP2)
    addGateWaysLayer(viewWP2)
    addLPlanQJMonitoringLayer(viewWP2)
    addLPlanJFMonitoringLayer(viewWP2)
    addLPlanACMonitoringLayer(viewWP2)
    addLPlanLEAKMonitoringLayer(viewWP2)
    addPlanLQJMonitoringLayer(viewWP2)

    print ("--- Add cross profile map ---")
    viewWP3 = addCrossMap()      
    addGateWaysLayer(viewWP3)
    addCRQJMonitoringLayer(viewWP3)
    addCRJFMonitoringLayer(viewWP3)
    addCRLEAKMonitoringLayer(viewWP3)
    addCRLQJMonitoringLayer(viewWP3)
    addCRACMonitoringLayer(viewWP3)
	
    print ("--- Add a empty longitudinal profile map ---")
    emap = is3.EngineeringMap('profile1', 0, 0, 100, 100, 0.01)
    emap.MapType = is3.EngineeringMapType.GeneralProfileMap;
    safe_view = is3.MainframeWrapper.addView(emap)
    tilefile = is3.Runtime.tilePath + "\\Empty.tpk"
    safe_view.addLocalTiledLayer(tilefile, 'baselayer')
	
	
    print ("--- Add a empty longitudinal profile map2 ---")
    emap = is3.EngineeringMap('profile2', 0, 0, 100, 100, 0.01)
    emap.MapType = is3.EngineeringMapType.GeneralProfileMap;
    safe_view = is3.MainframeWrapper.addView(emap)
    tilefile = is3.Runtime.tilePath + "\\Empty.tpk"
    safe_view.addLocalTiledLayer(tilefile, 'baselayer')
	
    print ("--- Add 3D map ---")
    viewWP3 = add3dview()

Load()
