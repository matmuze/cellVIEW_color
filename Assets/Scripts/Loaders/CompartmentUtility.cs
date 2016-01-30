using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Loaders;
using Newtonsoft.Json;

namespace Assets.Scripts.Loaders
{
    public class CompartmentUtility
    {
        public const string PATH_SEPARATOR = ".";

        public static Compartment DeserializeJson(string path)
        {
            var str = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Compartment>(str);
        }

        public static void PostProcessSceneGraph(Compartment rootCompartment)
        {
            var compartments = new List<Compartment>();
            var queue = new Queue<Compartment>();

            rootCompartment.parent_id = -1;
            rootCompartment.path = "root";

            queue.Enqueue(rootCompartment);

            while (queue.Count > 0)
            {
                var currentCompartment = queue.Dequeue();
                compartments.Add(currentCompartment);

                foreach (var ingredientGroup in currentCompartment.IngredientGroups)
                {
                    ingredientGroup.compartment_id = currentCompartment.unique_id;
                    ingredientGroup.path = currentCompartment.path + PATH_SEPARATOR + ingredientGroup.name;

                    foreach (var ingredient in ingredientGroup.Ingredients)
                    {
                        ingredient.name = ingredient.name.Split(new[] {"__"}, StringSplitOptions.RemoveEmptyEntries).Last().Trim();
                        ingredient.ingredient_group_id = ingredientGroup.unique_id;
                        ingredient.path = ingredientGroup.path + PATH_SEPARATOR + ingredient.name;
                    }
                }

                foreach (var compartment in currentCompartment.Compartments)
                {
                    compartment.parent_id = currentCompartment.unique_id;
                    compartment.path = currentCompartment.path + PATH_SEPARATOR + compartment.name;
                    queue.Enqueue(compartment);
                }
            }
        }

        public static List<IngredientGroup> GetAllIngredientGroups(Compartment rootCompartment)
        {
            var groups = new List<IngredientGroup>();
            var queue = new Queue<Compartment>();

            queue.Enqueue(rootCompartment);

            while (queue.Count > 0)
            {
                var currentCompartment = queue.Dequeue();
                groups.AddRange(currentCompartment.IngredientGroups);

                foreach (var compartment in currentCompartment.Compartments)
                {
                    queue.Enqueue(compartment);
                }
            }

            return groups;
        }


        //public static List<Compartment> GetAllCompartments(Compartment rootCompartment)
        //{
        //    var compartments = new List<Compartment>();
        //    var queue = new Queue<Compartment>();

        //    rootCompartment.parent_id = -1;
        //    rootCompartment.path = "root";

        //    queue.Enqueue(rootCompartment);

        //    while (queue.Count > 0)
        //    {
        //        var current = queue.Dequeue();
        //        compartments.Add(current);

        //        foreach (var compartment in current.Compartments)
        //        {
        //            compartment.parent_id = current.unique_id;
        //            compartment.path = current.path + PATH_SEPARATOR + compartment.name;
        //            queue.Enqueue(compartment);
        //        }
        //    }

        //    return compartments;
        //}

        //public static List<IngredientGroup> GetAllIngredientGroups(List<Compartment> compartments)
        //{
        //    var ingredientGroups = new List<IngredientGroup>();

        //    foreach (var compartment in compartments)
        //    {
        //        foreach (var ingredientGroup in compartment.IngredientGroups)
        //        {
        //            ingredientGroup.compartment_id = compartment.unique_id;
        //            ingredientGroup.path = compartment.path + PATH_SEPARATOR + ingredientGroup.name;
        //            ingredientGroups.Add(ingredientGroup);
        //        }
        //    }

        //    return ingredientGroups;
        //}

        //public static List<Ingredient> GetAllProteinIngredients(List<IngredientGroup> ingredientGroups)
        //{
        //    var ingredients = new List<Ingredient>();

        //    foreach (var ingredientGroup in ingredientGroups)
        //    {
        //        if (ingredientGroup.group_type == (int)IngredientGroupType.INTERIOR_PROTEINS
        //            || ingredientGroup.group_type == (int)IngredientGroupType.SURFACE_PROTEINS)
        //        {
        //            foreach (var ingredient in ingredientGroup.Ingredients)
        //            {
        //                ingredient.ingredient_group_id = ingredientGroup.unique_id;
        //                ingredient.path = ingredientGroup.path + PATH_SEPARATOR + ingredient.name;
        //                ingredients.Add(ingredient);
        //            }
        //        }
        //    }

        //    return ingredients;
        //}
    }

    public enum IngredientType { PROTEIN = 0, LIPID, FIBER };
    public enum IngredientGroupType { INTERIOR_PROTEINS = 0, SURFACE_PROTEINS, LIPIDS, FIBERS };

    [Serializable]
    public class Compartment
    {
        public string name { get; set; }
        public string path { get; set; }
        
        public int local_id { get; set; }
        public int unique_id { get; set; }
        public int parent_id { get; set; }

        public List<IngredientGroup> IngredientGroups { get; set; }

        public List<Compartment> Compartments
        {
            get { return _compartments; }
            set { _compartments = value; }
        }

        [NonSerialized] private List<Compartment> _compartments;
    }

    [Serializable]
    public class IngredientGroup
    {
        public bool fiber { get; set; }
        public string name { get; set; }
        public string path { get; set; }

        public int local_id { get; set; }
        public int unique_id { get; set; }
        public int group_type { get; set; }
        public int compartment_id { get; set; }

        public List<Ingredient> Ingredients { get; set; }
    }

    [Serializable]
    public class Ingredient
    {
        public string name { get; set; }
        public string path { get; set; }

        public int ingredient_id { get; set; }
        public int ingredient_type { get; set; }
        public int ingredient_group_id { get; set; }

        public int nbMol { get; set; }
        public Source source { get; set; }
    }

    [Serializable]
    public class Source
    {
        public bool biomt { get; set; }
        public string pdb { get; set; }
        public Transform transform { get; set; }
    }

    [Serializable]
    public class Transform
    {
        public bool center { get; set; }
        //public List<double> rotate { get; set; }
        //public List<double> translate { get; set; }
    }
}
