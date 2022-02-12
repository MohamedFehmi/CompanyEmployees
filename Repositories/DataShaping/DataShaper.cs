using Contracts;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DataShaping
{
    public class DataShaper<T> : IDataShaper<T> where T : class
    {
        public PropertyInfo[] Properties { get; set; }

        public DataShaper()
        {
            Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        }
        
<<<<<<< HEAD
        public IEnumerable<ShapedEntity> ShapeData(IEnumerable<T> entities, string fieldsString)
=======
        public IEnumerable<Entity> ShapeData(IEnumerable<T> entities, string fieldsString)
>>>>>>> c86bfd419cec1f4e52cccde66549f9ca1c243b15
        {
            //Extract names of properties from QueryString
            var requiredProperties = GetRequiredProperties(fieldsString);

<<<<<<< HEAD
            //Build the ShapedEntity collection
            return FetchData(entities, requiredProperties);
        }

        public ShapedEntity ShapeData(T entity, string fieldsString)
=======
            //Build the Entity collection
            return FetchData(entities, requiredProperties);
        }

        public Entity ShapeData(T entity, string fieldsString)
>>>>>>> c86bfd419cec1f4e52cccde66549f9ca1c243b15
        {
            var requiredProperties = GetRequiredProperties(fieldsString);

            return FetchDataForEntity(entity, requiredProperties);
        }
        
        private IEnumerable<PropertyInfo> GetRequiredProperties(string fieldsString)
        {
            var requiredProperties = new List<PropertyInfo>();

            if (!string.IsNullOrWhiteSpace(fieldsString))
            {
                var fields = fieldsString.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var field in fields)
                {
                    var property = Properties.FirstOrDefault(pi => pi.Name.Equals(field.Trim(), StringComparison.InvariantCultureIgnoreCase));

                    if (property == null)
                        continue;

                    requiredProperties.Add(property);
                }
            }
            else
            {
                requiredProperties = Properties.ToList();
            }

            return requiredProperties;
        }

<<<<<<< HEAD
        private IEnumerable<ShapedEntity> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedData = new List<ShapedEntity>();
=======
        private IEnumerable<Entity> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedData = new List<Entity>();
>>>>>>> c86bfd419cec1f4e52cccde66549f9ca1c243b15

            foreach (var entity in entities)
            {
                var shapedObject = FetchDataForEntity(entity, requiredProperties);
                shapedData.Add(shapedObject);
            }

            return shapedData;
        }

<<<<<<< HEAD
        private ShapedEntity FetchDataForEntity(T entity, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedObject = new ShapedEntity();
=======
        private Entity FetchDataForEntity(T entity, IEnumerable<PropertyInfo> requiredProperties)
        {
            var shapedData = new Entity();
>>>>>>> c86bfd419cec1f4e52cccde66549f9ca1c243b15

            foreach (var property in requiredProperties)
            {
                var objectPropertyValue = property.GetValue(entity);
<<<<<<< HEAD
                shapedObject.Entity.TryAdd(property.Name, objectPropertyValue);
            }

            var objectProperty = entity.GetType().GetProperty("EmployeeID");
            shapedObject.Id = (Guid)objectProperty.GetValue(entity);

            return shapedObject;
=======
                shapedData.TryAdd(property.Name, objectPropertyValue);
            }

            return shapedData;
>>>>>>> c86bfd419cec1f4e52cccde66549f9ca1c243b15
        }
    }
}
