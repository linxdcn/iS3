using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS3.SimpleStructureTools.Helper.FEM.FEMModel
{
    public class Model
    {
        private Dictionary<int, Node> _nodes;
        private Dictionary<int, Element> _eIDtoElement;
        private Dictionary<int, Dictionary<int, Element>> _partIDtoElements;

        private Dictionary<int, Part> _parts;
        private Dictionary<int, ElementType> _elementTypes;
        private Dictionary<int, Mat> _mats;
        private Dictionary<int, Section> _sections;

        private List<Boundary> _boundaries;
        private List<Constrained> _constraineds;
        private List<Load> _loads;


        public Model()
        {
            _nodes = new Dictionary<int, Node>();
            _eIDtoElement = new Dictionary<int, Element>();
            _partIDtoElements = new Dictionary<int, Dictionary<int, Element>>();

            _elementTypes = new Dictionary<int, ElementType>();
            _parts = new Dictionary<int, Part>();
            _mats = new Dictionary<int, Mat>();
            _sections = new Dictionary<int, Section>();

            _boundaries = new List<Boundary>();
            _constraineds = new List<Constrained>();
        }

        public Dictionary<int, Node> nodes { get { return _nodes; } }
        public Dictionary<int, Element> eIDtoElement { get { return _eIDtoElement; } }
        public Dictionary<int, Dictionary<int, Element>> partIDtoElements { get { return _partIDtoElements; } }
        public Dictionary<int, Part> parts { get { return _parts; } }
        public Dictionary<int, ElementType> elementTypes {get { return _elementTypes;}}
        public Dictionary<int, Mat> mats { get { return _mats; } }
        public Dictionary<int, Section> sections { get { return _sections; } }
        public List<Boundary> boundaries { get { return _boundaries; } }
        public List<Constrained> constradineds { get { return _constraineds; } }
        public List<Load> loads { get { return _loads; } }

        public void AddPart(Part part)
        {
            if (_parts.ContainsKey(part.pid))
                return;
            _parts[part.pid] = part;
        }
        public void AddElementType(ElementType type)
        {
            if (_elementTypes.ContainsKey(type.eid))
                return;
            _elementTypes[type.eid] = type;
        }
        public void AddMat(Mat mat)
        {
            if (_mats.ContainsKey(mat.mid))
                return;
            _mats[mat.mid] = mat;
        }
        public void AddSection(Section section)
        {
            if (_sections.ContainsKey(section.secid))
                return;
            _sections[section.secid] = section;
        }

        public void AddNode(Node node)
        {
            if (_nodes.ContainsKey(node.nid))
                return;
            _nodes[node.nid] = node;
        }
        public void AddElement(Element element)
        {
            if (_eIDtoElement.ContainsKey(element.eid))
                return;
            _eIDtoElement[element.eid] = element;
            if (_partIDtoElements.ContainsKey(element.pid))
                _partIDtoElements[element.pid][element.eid] = element;
            else
            {
                _partIDtoElements[element.pid] = new Dictionary<int, Element>();
                _partIDtoElements[element.pid][element.eid] = element;
            }
        }
        public void AddBoundary(Boundary boundary)
        {
            _boundaries.Add(boundary);
        }
        public void AddConstrained(Constrained constrained)
        {
            _constraineds.Add(constrained);
        }
        public void AddLoad(Load load)
        {
            _loads.Add(load);
        }
    }
}
