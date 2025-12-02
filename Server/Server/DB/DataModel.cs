using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    // 클래스 이름이 아닌 어노테이션 이름으로 테이블 만들어줌
    [Table("Account")]
    public class AccountDb
    {
        public int AccountDbId { get; set; }    // ByConvention 방법으로 클래스 이름에 Id 붙이면 자동으로 pk됨
        public string AccountName { get; set; } // 일단은 Unique 하게 설정 (Index 해줌)
        public ICollection<PlayerDb> Players { get; set; }  // 이렇게 하면 Player 테이블에 FK 컬럼이 생성됨 
    }

    [Table("Player")]
    public class PlayerDb
    {
        public int PlayerDbId { get; set; }
        public string PlayerName { get; set; }  // 일단은 Unique 하게 설정 (Index 해줌)
        public AccountDb Account { get; set; }
    }
}
