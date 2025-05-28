namespace PlayerPortal.Data.DataTransferModels
{
    public class PlayerDataTransferModel
    {
        public int Id { get; set; }
        public int ShirtNo { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Appearance { get; set; }
        public int Goals { get; set; }

        public PlayerDataTransferModel() { }

        public PlayerDataTransferModel(int id, int shirtNo, string name, int appearance, int goals)
        {
            Id = id;
            ShirtNo = shirtNo;
            Name = name;
            Appearance = appearance;
            Goals = goals;
        }
    }
}