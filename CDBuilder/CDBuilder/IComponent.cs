using System.Collections.Generic;

namespace CDBuilder
{
    interface IComponent
    {
        void Handle(IEnumerable<string> args);
    }
}