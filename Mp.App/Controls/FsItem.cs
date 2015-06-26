using System;
using Microsoft.SPOT;
using System.Collections;

namespace Mp.App.Controls
{
    class FsItem
    {
        public string Name;
        public string Path;
        public bool IsDirectory;
        public bool IsBack;
        public bool IsMusicFile;

        public FsItem Parent { get; private set; }
        public ArrayList Childs { get; set; }

        public FsItem(FsItem parent)
        {
            Parent = parent;
            Childs = null;
        }

        public FsItem GetRootNode()
        {
            FsItem ret = this;
            while (ret.Parent != null) ret = ret.Parent;
            return ret;
        }
    }
}
