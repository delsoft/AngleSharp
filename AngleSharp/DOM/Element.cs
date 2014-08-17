﻿namespace AngleSharp.DOM
{
    using AngleSharp.DOM.Collections;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents an element node.
    /// </summary>
    class Element : Node, IElement
    {
        #region Fields

        readonly HtmlElementCollection _elements;
        readonly AttrContainer _attributes;

        String _prefix;
        String _namespace;
        TokenList _classList;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new element node.
        /// </summary>
        internal Element(String name, NodeFlags flags = NodeFlags.None)
            : base(name, NodeType.Element, flags)
        {
            _elements = new HtmlElementCollection(this, deep: false);
            _attributes = new AttrContainer();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the namespace prefix of the specified node, or null if no prefix is specified.
        /// </summary>
        public String Prefix
        {
            get { return _prefix; }
            internal set { _prefix = value; }
        }

        /// <summary>
        /// Gets the local part of the qualified name of this node.
        /// </summary>
        public String LocalName
        {
            get { return NodeName; }
        }

        /// <summary>
        /// Gets the namespace URI of this node.
        /// </summary>
        public String NamespaceUri
        {
            get { return _namespace; }
            internal set { _namespace = value; }
        }

        /// <summary>
        /// Gets or sets the text content of a node and its descendants.
        /// </summary>
        public sealed override String TextContent
        {
            get
            {
                var sb = Pool.NewStringBuilder();

                foreach (var child in this.GetDescendentsOf().OfType<IText>())
                    sb.Append(child.Data);

                return sb.ToPool();
            }
            set
            {
                var node = !String.IsNullOrEmpty(value) ? new TextNode(value) { Owner = Owner } : null;
                ReplaceAll(node, false);
            }
        }

        /// <summary>
        /// Gets the list of class names.
        /// </summary>
        public ITokenList ClassList
        {
            get { return _classList ?? (_classList = new TokenList(this, AttributeNames.Class)); }
        }

        /// <summary>
        /// Gets or sets the value of the class attribute.
        /// </summary>
        public String ClassName
        {
            get { return GetAttribute(AttributeNames.Class); }
            set { SetAttribute(AttributeNames.Class, value); }
        }

        /// <summary>
        /// Gets or sets the value of the id attribute.
        /// </summary>
        public String Id
        {
            get { return GetAttribute(AttributeNames.Id); }
            set { SetAttribute(AttributeNames.Id, value); }
        }

        /// <summary>
        /// Gets the tagname of the element.
        /// </summary>
        public String TagName
        {
            get { return NodeName; }
        }

        /// <summary>
        /// Gets the element immediately preceding in this node's parent's list of nodes, 
        /// null if the current element is the first element in that list.
        /// </summary>
        public IElement PreviousElementSibling
        {
            get
            {
                var parent = Parent;

                if (parent != null)
                {
                    var found = false;

                    for (int i = parent.ChildNodes.Length - 1; i >= 0; i--)
                    {
                        if (parent.ChildNodes[i] == this)
                            found = true;
                        else if (found && parent.ChildNodes[i] is IElement)
                            return (IElement)parent.ChildNodes[i];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the element immediately following in this node's parent's list of nodes,
        /// or null if the current element is the last element in that list.
        /// </summary>
        public IElement NextElementSibling
        {
            get
            {
                var parent = Parent;

                if (parent != null)
                {
                    var n = parent.ChildNodes.Length;
                    var found = false;

                    for (int i = 0; i < n; i++)
                    {
                        if (parent.ChildNodes[i] == this)
                            found = true;
                        else if (found && parent.ChildNodes[i] is IElement)
                            return (IElement)parent.ChildNodes[i];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the number of child elements.
        /// </summary>
        public Int32 ChildElementCount
        {
            get { return _elements.Length; }
        }

        /// <summary>
        /// Gets the child elements.
        /// </summary>
        public IHtmlCollection Children
        {
            get { return _elements; }
        }

        /// <summary>
        /// Gets the first child element of this element.
        /// </summary>
        public IElement FirstElementChild
        {
            get 
            {
                var children = ChildNodes;
                var n = children.Length;

                for (int i = 0; i < n; i++)
                {
                    var child = children[i] as IElement;

                    if (child != null)
                        return child;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the last child element of this element.
        /// </summary>
        public IElement LastElementChild
        {
            get
            {
                var children = ChildNodes;

                for (int i = children.Length - 1; i >= 0; i--)
                {
                    var child = children[i] as IElement;

                    if (child != null)
                        return child;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the HTML syntax describing the element's descendants.
        /// </summary>
        public String InnerHtml
        {
            get { return ChildNodes.ToHtml(); }
            set { ReplaceAll(new DocumentFragment(value, this), false); }
        }

        /// <summary>
        /// Gets or sets the HTML syntax describing the element including its descendants. 
        /// </summary>
        public String OuterHtml
        {
            get { return ToHtml(); }
            set
            {
                var parent = Parent;

                if (parent != null)
                {
                    if (Owner != null && Owner.DocumentElement == this)
                        throw new DomException(ErrorCode.NoModificationAllowed);

                    parent.InsertChild(parent.IndexOf(this), new DocumentFragment(value, this));
                    parent.RemoveChild(this);
                }
                else
                    throw new DomException(ErrorCode.NotSupported);
            }
        }
        
        /// <summary>
        /// Gets the sequence of associated attributes.
        /// </summary>
        IEnumerable<IAttr> IElement.Attributes
        {
            get { return _attributes; }
        }

        /// <summary>
        /// Gets the associated attribute container.
        /// </summary>
        internal AttrContainer Attributes
        {
            get { return _attributes; }
        }

        #endregion

        #region Design Properties

        /// <summary>
        /// Gets if the element is being hovered.
        /// </summary>
        [DomName("isHovered")]
        public Boolean IsHovered
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets if the element has currently focus.
        /// </summary>
        [DomName("isFocused")]
        public Boolean IsFocused
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the width of the left border of this element.
        /// </summary>
        [DomName("clientLeft")]
        public Int32 ClientLeft
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the height of the top border of this element.
        /// </summary>
        [DomName("clientTop")]
        public Int32 ClientTop
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the inner width of this element.
        /// </summary>
        [DomName("clientWidth")]
        public Int32 ClientWidth
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the inner height of this element.
        /// </summary>
        [DomName("clientHeight")]
        public Int32 ClientHeight
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the element from which all offset calculations are currently computed.
        /// </summary>
        [DomName("offsetParent")]
        public Element OffsetParent
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the distance from this element's left border to its offsetParent's left border.
        /// </summary>
        [DomName("offsetLeft")]
        public Int32 OffsetLeft
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the distance from this element's top border to its offsetParent's top border.
        /// </summary>
        [DomName("offsetTop")]
        public Int32 OffsetTop
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the width of this element, relative to the layout.
        /// </summary>
        [DomName("offsetWidth")]
        public Int32 OffsetWidth
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the height of this element, relative to the layout.
        /// </summary>
        [DomName("offsetHeight")]
        public Int32 OffsetHeight
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the left scroll offset of an element.
        /// </summary>
        [DomName("scrollLeft")]
        public Int32 ScrollLeft
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the top scroll offset of an element.
        /// </summary>
        [DomName("scrollTop")]
        public Int32 ScrollTop
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the scroll view width of an element.
        /// </summary>
        [DomName("scrollWidth")]
        public Int32 ScrollWidth
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the scroll view height of an element.
        /// </summary>
        [DomName("scrollHeight")]
        public Int32 ScrollHeight
        {
            get;
            internal set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the first element within the document (using depth-first pre-order traversal
        /// of the document's nodes) that matches the specified group of selectors.
        /// </summary>
        /// <param name="selectors">A string containing one or more CSS selectors separated by commas.</param>
        /// <returns>An element object.</returns>
        public IElement QuerySelector(String selectors)
        {
            return ChildNodes.QuerySelector(selectors);
        }

        /// <summary>
        /// Returns a list of the elements within the document (using depth-first pre-order traversal
        /// of the document's nodes) that match the specified group of selectors.
        /// </summary>
        /// <param name="selectors">A string containing one or more CSS selectors separated by commas.</param>
        /// <returns>A collection of HTML elements.</returns>
        public IHtmlCollection QuerySelectorAll(String selectors)
        {
            return ChildNodes.QuerySelectorAll(selectors);
        }

        /// <summary>
        /// Returns a set of elements which have all the given class names.
        /// </summary>
        /// <param name="classNames">A string representing the list of class names to match; class names are separated by whitespace.</param>
        /// <returns>A collection of HTML elements.</returns>
        public IHtmlCollection GetElementsByClassName(String classNames)
        {
            return ChildNodes.GetElementsByClassName(classNames);
        }

        /// <summary>
        /// Returns a NodeList of elements with the given tag name. The complete document is searched, including the root node.
        /// </summary>
        /// <param name="tagName">A string representing the name of the elements. The special string "*" represents all elements.</param>
        /// <returns>A NodeList of found elements in the order they appear in the tree.</returns>
        public IHtmlCollection GetElementsByTagName(String tagName)
        {
            return ChildNodes.GetElementsByTagName(tagName);
        }

        /// <summary>
        /// Returns a list of elements with the given tag name belonging to the given namespace.
        /// The complete document is searched, including the root node.
        /// </summary>
        /// <param name="namespaceURI">The namespace URI of elements to look for.</param>
        /// <param name="tagName">Either the local name of elements to look for or the special value "*", which matches all elements.</param>
        /// <returns>A NodeList of found elements in the order they appear in the tree.</returns>
        public IHtmlCollection GetElementsByTagNameNS(String namespaceURI, String tagName)
        {
            return ChildNodes.GetElementsByTagNameNS(namespaceURI, tagName);
        }

        public Boolean Matches(String selectors)
        {
            return AngleSharp.Parser.Css.CssParser.ParseSelector(selectors).Match(this);
        }

        /// <summary>
        /// Returns a duplicate of the node on which this method was called.
        /// </summary>
        /// <param name="deep">Optional value: true if the children of the node should also be cloned, or false to clone only the specified node.</param>
        /// <returns>The duplicate node.</returns>
        public override INode Clone(Boolean deep = true)
        {
            var node = new Element(NodeName, Flags);
            CopyProperties(this, node, deep);
            CopyAttributes(this, node);
            return node;
        }

        /// <summary>
        /// Returns a boolean value indicating whether the specified element has the specified attribute or not.
        /// </summary>
        /// <param name="name">The attributes name.</param>
        /// <returns>The return value of true or false.</returns>
        public Boolean HasAttribute(String name)
        {
            if (_namespace == Namespaces.HtmlUri)
                name = name.ToLower();

            for (int i = 0; i < _attributes.Count; i++)
            {
                if (_attributes[i].Name == name)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a boolean value indicating whether the specified element has the specified attribute or not.
        /// </summary>
        /// <param name="namespaceUri">A string specifying the namespace of the attribute.</param>
        /// <param name="localName">The attributes name.</param>
        /// <returns>The return value of true or false.</returns>
        public Boolean HasAttribute(String namespaceUri, String localName)
        {
            if (String.IsNullOrEmpty(namespaceUri))
                namespaceUri = null;

            for (int i = 0; i < _attributes.Count; i++)
            {
                if (_attributes[i].LocalName == localName && _attributes[i].NamespaceUri == namespaceUri)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the value of the named attribute on the specified element.
        /// </summary>
        /// <param name="name">The name of the attribute whose value you want to get.</param>
        /// <returns>If the named attribute does not exist, the value returned will be null, otherwise the attribute's value.</returns>
        public String GetAttribute(String name)
        {
            if (_namespace == Namespaces.HtmlUri)
                name = name.ToLower();

            for (int i = 0; i < _attributes.Count; i++)
            {
                if (_attributes[i].Name == name)
                    return _attributes[i].Value;
            }

            return null;
        }

        /// <summary>
        /// Returns the value of the named attribute on the specified element.
        /// </summary>
        /// <param name="namespaceUri">A string specifying the namespace of the attribute.</param>
        /// <param name="localName">The name of the attribute whose value you want to get.</param>
        /// <returns>If the named attribute does not exist, the value returned will be null, otherwise the attribute's value.</returns>
        public String GetAttribute(String namespaceUri, String localName)
        {
            if (String.IsNullOrEmpty(namespaceUri))
                namespaceUri = null;

            for (int i = 0; i < _attributes.Count; i++)
            {
                if (_attributes[i].LocalName == localName && _attributes[i].NamespaceUri == namespaceUri)
                    return _attributes[i].Value;
            }

            return null;
        }

        /// <summary>
        /// Adds a new attribute or changes the value of an existing attribute on the specified element.
        /// </summary>
        /// <param name="name">The name of the attribute as a string.</param>
        /// <param name="value">The desired new value of the attribute.</param>
        public void SetAttribute(String name, String value)
        {
            if (value != null)
            {
                if (!name.IsXmlName())
                    throw new DomException(ErrorCode.InvalidCharacter);

                if (_namespace == Namespaces.HtmlUri)
                    name = name.ToLower();

                for (int i = 0; i < _attributes.Count; i++)
                {
                    if (_attributes[i].Name == name)
                    {
                        _attributes[i].Value = value;
                        //TODO
                        // Queue a mutation record of "attributes" for element with name attribute's
                        // local name, namespace attribute's namespace, and oldValue attribute's value.
                        return;
                    }
                }

                //TODO
                // Queue a mutation record of "attributes" for element with name attribute's
                // local name, namespace attribute's namespace, and oldValue null.
                _attributes.Add(new Attr(_attributes, name, value));
            }
            else
                RemoveAttribute(name);
        }

        /// <summary>
        /// Adds a new attribute or changes the value of an existing attribute on the specified element.
        /// </summary>
        /// <param name="namespaceUri">A string specifying the namespace of the attribute.</param>
        /// <param name="name">The name of the attribute as a string.</param>
        /// <param name="value">The desired new value of the attribute.</param>
        public void SetAttribute(String namespaceUri, String name, String value)
        {            
            if (value != null)
            {
                if (String.IsNullOrEmpty(namespaceUri))
                    namespaceUri = null;

                if (!name.IsXmlName())
                    throw new DomException(ErrorCode.InvalidCharacter);
                else if (!name.IsQualifiedName())
                    throw new DomException(ErrorCode.Namespace);

                var index = name.IndexOf(Specification.Colon);
                var prefix = index >= 0 ? name.Substring(0, index) : null;
                var localName = index >= 0 ? name.Substring(index + 1) : name;

                if (prefix != null && namespaceUri == null)
                    throw new DomException(ErrorCode.Namespace);

                if (prefix == Namespaces.XmlPrefix && namespaceUri != Namespaces.XmlUri)
                    throw new DomException(ErrorCode.Namespace);
                else if ((name == Namespaces.XmlNsPrefix || prefix == Namespaces.XmlNsPrefix) && namespaceUri != Namespaces.XmlNsUri)
                    throw new DomException(ErrorCode.Namespace);
                else if (namespaceUri == Namespaces.XmlNsUri && (name != Namespaces.XmlNsPrefix || prefix != Namespaces.XmlNsPrefix))
                    throw new DomException(ErrorCode.Namespace);

                for (int i = 0; i < _attributes.Count; i++)
                {
                    if (_attributes[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        _attributes[i].Value = value;
                        //TODO
                        // Queue a mutation record of "attributes" for element with name attribute's
                        // local name, namespace attribute's namespace, and oldValue attribute's value.
                        return;
                    }
                }

                //TODO
                // Queue a mutation record of "attributes" for element with name attribute's
                // local name, namespace attribute's namespace, and oldValue null.
                _attributes.Add(new Attr(_attributes, prefix, localName, value, namespaceUri));
            }
            else
                RemoveAttribute(namespaceUri, name);
        }

        /// <summary>
        /// Removes an attribute from the specified element.
        /// </summary>
        /// <param name="name">Is a string that names the attribute to be removed.</param>
        /// <returns>The current element.</returns>
        public void RemoveAttribute(String name)
        {
            if (_namespace == Namespaces.HtmlUri)
                name = name.ToLower();

            for (int i = 0; i < _attributes.Count; i++)
            {
                if (_attributes[i].Name == name)
                {
                    _attributes.RemoveAt(i);
                    //TODO
                    // Queue a mutation record of "attributes" for element with name attribute's
                    // local name, namespace attribute's namespace, and oldValue attribute's value.
                    return;
                }
            }
        }

        /// <summary>
        /// Removes an attribute from the specified element.
        /// </summary>
        /// <param name="namespaceUri">A string specifying the namespace of the attribute.</param>
        /// <param name="localName">Is a string that names the attribute to be removed.</param>
        /// <returns>The current element.</returns>
        public void RemoveAttribute(String namespaceUri, String localName)
        {
            if (String.IsNullOrEmpty(namespaceUri))
                namespaceUri = null;

            for (int i = 0; i < _attributes.Count; i++)
            {
                if (_attributes[i].LocalName == localName && _attributes[i].NamespaceUri == namespaceUri)
                {
                    _attributes.RemoveAt(i);
                    //TODO
                    // Queue a mutation record of "attributes" for element with name attribute's
                    // local name, namespace attribute's namespace, and oldValue attribute's value.
                    return;
                }
            }
        }

        /// <summary>
        /// Prepends nodes to the current node.
        /// </summary>
        /// <param name="nodes">The nodes to prepend.</param>
        public void Prepend(params INode[] nodes)
        {
            this.PrependNodes(nodes);
        }

        /// <summary>
        /// Appends nodes to current node.
        /// </summary>
        /// <param name="nodes">The nodes to append.</param>
        public void Append(params INode[] nodes)
        {
            this.AppendNodes(nodes);
        }

        public override Boolean IsEqualNode(INode otherNode)
        {
            var otherElement = otherNode as IElement;

            if (otherElement != null)
            {
                if (this.NamespaceUri != otherElement.NamespaceUri)
                    return false;

                if (_attributes.Count != otherElement.Attributes.Count())
                    return false;

                for (int i = 0; i < _attributes.Count; i++)
                {
                    if (!otherElement.Attributes.Any(m => m.Name == _attributes[i].Name && m.Value == _attributes[i].Value))
                        return false;
                }

                return base.IsEqualNode(otherNode);
            }

            return false;
        }

        /// <summary>
        /// Inserts nodes before the current node.
        /// </summary>
        /// <param name="nodes">The nodes to insert before.</param>
        /// <returns>The current element.</returns>
        public void Before(params INode[] nodes)
        {
            this.InsertBefore(nodes);
        }

        /// <summary>
        /// Inserts nodes after the current node.
        /// </summary>
        /// <param name="nodes">The nodes to insert after.</param>
        /// <returns>The current element.</returns>
        public void After(params INode[] nodes)
        {
            this.InsertAfter(nodes);
        }

        /// <summary>
        /// Replaces the current node with the nodes.
        /// </summary>
        /// <param name="nodes">The nodes to replace.</param>
        public void Replace(params INode[] nodes)
        {
            this.ReplaceWith(nodes);
        }

        /// <summary>
        /// Removes the current element from the parent.
        /// </summary>
        public void Remove()
        {
            this.RemoveFromParent();
        }

        /// <summary>
        /// Inserts new HTML elements specified by the given HTML string at
        /// a position relative to the current element specified by the position.
        /// </summary>
        /// <param name="position">The relation to the current element.</param>
        /// <param name="html">The HTML code to generate elements for.</param>
        public void Insert(AdjacentPosition position, String html)
        {
            var useThis = position == AdjacentPosition.BeforeBegin || position == AdjacentPosition.AfterEnd;
            var nodeParent = useThis ? this : Parent as Element;
            var nodes = new DocumentFragment(html, nodeParent);

            switch (position)
            {
                case AdjacentPosition.BeforeBegin:
                    Parent.InsertBefore(nodes, this);
                    break;

                case AdjacentPosition.AfterEnd:
                    Parent.InsertChild(Parent.IndexOf(this) + 1, nodes);
                    break;

                case AdjacentPosition.AfterBegin:
                    InsertChild(0, nodes);
                    break;

                case AdjacentPosition.BeforeEnd:
                    AppendChild(nodes);
                    break;
            }
        }

        #endregion

        #region String Representation

        /// <summary>
        /// Returns an HTML-code representation of the node.
        /// </summary>
        /// <returns>A string containing the HTML code.</returns>
        public override String ToHtml()
        {
            var sb = Pool.NewStringBuilder();
            var tagName = (Flags & (NodeFlags.HtmlMember | NodeFlags.SvgMember | NodeFlags.MathMember)) != NodeFlags.None ? LocalName : NodeName;

            sb.Append(Specification.LessThan).Append(tagName);

            foreach (var attribute in _attributes)
                sb.Append(Specification.Space).Append(attribute.ToString());

            sb.Append(Specification.GreaterThan);

            if (!Flags.HasFlag(NodeFlags.SelfClosing))
            {
                if (Flags.HasFlag(NodeFlags.LineTolerance) && FirstChild is IText)
                {
                    var text = (IText)FirstChild;

                    if (text.Data.Length > 0 && text.Data[0] == Specification.LineFeed)
                        sb.Append(Specification.LineFeed);
                }

                foreach (var child in ChildNodes)
                    sb.Append(child.ToHtml());

                sb.Append(Specification.LessThan).Append(Specification.Solidus).Append(tagName);
                sb.Append(Specification.GreaterThan);
            }

            return sb.ToPool();
        }

        #endregion

        #region Helpers

        internal override void Close()
        {
            base.Close();
            OnAttributeChanged(AttributeNames.Class, value =>
            {
                if (_classList != null)
                    _classList.Update(value);
            });
        }

        /// <summary>
        /// Adds a new attribute if the attribute is not yet created.
        /// Does not fire the changed event.
        /// </summary>
        /// <param name="name">The name of the attribute as a string.</param>
        /// <param name="value">The desired new value of the attribute.</param>
        internal void AddAttribute(String name, String value)
        {
            if (!_attributes.Has(name))
                _attributes.Add(new Attr(_attributes, name, value));
        }

        protected sealed override String LocateNamespace(String prefix)
        {
            return ElementExtensions.LocateNamespace(this, prefix);
        }

        protected sealed override String LocatePrefix(String namespaceUri)
        {
            return ElementExtensions.LocatePrefix(this, namespaceUri);
        }

        /// <summary>
        /// Copies the attributes from the source element to the target element.
        /// Each attribute will be recreated on the target.
        /// </summary>
        /// <param name="source">The source of the attributes.</param>
        /// <param name="target">The target where to create the attributes.</param>
        protected static void CopyAttributes(Element source, Element target)
        {
            target._namespace = source._namespace;
            target._prefix = source._prefix;

            for (int i = 0; i < source._attributes.Count; i++)
                target.SetAttribute(source._attributes[i].Name, source._attributes[i].Value);
        }

        protected void OnAttributeChanged(String name, Action<String> callback)
        {
            _attributes.Changed += (s, ev) =>
            {
                if (ev.Name.Equals(name, StringComparison.Ordinal))
                    callback(ev.Value);
            };
        }

        #endregion
    }
}
