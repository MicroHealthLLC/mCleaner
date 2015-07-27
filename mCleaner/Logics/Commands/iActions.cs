using mCleaner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mCleaner.Logics.Commands
{
    public interface iActions
    {
        action Action { get; set; }
        void Enqueue(bool apply = false);
    }
}
