namespace AiTech.CrudPattern
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TOwner">Owner</typeparam>
    /// <typeparam name="TSubItem">SubItems</typeparam>
    public class ManyToManyEntity<TOwner,TSubItem>: Entity
    {
        public TOwner Owner {get; set; }

        public TSubItem SubItem { get; set; }

        public ManyToManyEntity()
        {
            //Owner = new TOwner();
            //SubItem = new TSubItem();
        }
    }
}
