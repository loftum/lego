namespace Lego.Server
{
    public class ChironController : LegoCarController
    {
        private readonly IChiron _chiron;

        public ChironController(IChiron chiron) : base(chiron)
        {
            _chiron = chiron;
        }
        
    }
}