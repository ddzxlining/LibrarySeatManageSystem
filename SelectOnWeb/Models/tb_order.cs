//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace SelectOnWeb.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tb_order
    {
        public string no { get; set; }
        public Nullable<short> room { get; set; }
        public Nullable<int> seat { get; set; }
        public byte[] operation_time { get; set; }
    
        public virtual tb_seat tb_seat { get; set; }
        public virtual tb_student tb_student { get; set; }
    }
}