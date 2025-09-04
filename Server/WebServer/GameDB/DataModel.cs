using System.ComponentModel.DataAnnotations.Schema;

namespace GameDB
{
	[Table("Test")]
	public class TestDb
	{
		// Convention : [클래스]Id 으로 명명하면 PK
		public int TestDbId { get; set; } // PK
		public string Name { get; set; }
	}
}
