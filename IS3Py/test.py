import is3

def output(text):
    is3.mainframe.output(text)

def loadGeoData():
    dbContext.Open()
    geo.loadAllObjects(dbContext)
    dbContext.Close()

def LoadStrData():
    dbContext.Open()
    struct.loadAllObjects(dbContext)
    dbContext.Close()


prj = is3.Project()
prj.loadDefinition("SHL12_TT.xml")
output(str(prj))

dbContext = prj.getDbContext()
geo = prj.getDomain(is3.DomainType.Geology)
struct = prj.getDomain(is3.DomainType.Structure)

loadGeoData()
objs = geo.objsContainer['AllWaterProperties']
boreholes = geo.objsContainer['Allboreholes']
strata = geo.objsContainer['AllStratum']
soilprops = geo.objsContainer['AllSoilProperties']
output(str(boreholes))

LoadStrData()
tunnels = struct.objsContainer['AllTunnelAxes']
SLs = struct.objsContainer['AllSegmentLinings']
output(str(tunnels))

