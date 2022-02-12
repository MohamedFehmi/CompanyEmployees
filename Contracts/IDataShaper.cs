using Entities.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IDataShaper<T>
    {
<<<<<<< HEAD
        IEnumerable<ShapedEntity> ShapeData(IEnumerable<T> entities, string fieldsString);
        ShapedEntity ShapeData(T entity, string fieldsString);
=======
        IEnumerable<Entity> ShapeData(IEnumerable<T> entities, string fieldsString);
        Entity ShapeData(T entity, string fieldsString);
>>>>>>> c86bfd419cec1f4e52cccde66549f9ca1c243b15
    }
}
