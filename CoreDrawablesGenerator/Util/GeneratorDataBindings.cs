using System.Collections.Generic;

namespace CoreDrawablesGenerator
{
    /// <summary>
    /// Data bindings for <see cref="MainWindow"/>.
    /// To update primitive types, update the control bound to the property (i.e. chk.IsChecked).
    /// </summary>
    public class GeneratorDataBindings
    {
        /// <summary>
        /// Gets a modifiable collection of templates.
        /// </summary>
        public List<Template> Templates { get; } = new List<Template>();

        /// <summary>
        /// Gets a value indicating whether white (FFFFFF) should be ignored when parsing images.
        /// </summary>
        public bool IgnoreWhite { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the 'weapon' group should be added to the templates.
        /// </summary>
        public bool WeaponGroup { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the 'inventoryIcon' should be added to the templates.
        /// </summary>
        public bool InventoryIcon { get; protected set; }
    }
}
