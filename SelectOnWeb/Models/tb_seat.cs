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
    
    public partial class tb_seat
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tb_seat()
        {
            this.tb_seat_student = new HashSet<tb_seat_student>();
            this.tb_order = new HashSet<tb_order>();
        }
    
        public int no { get; set; }
        public short room { get; set; }
        public Nullable<int> desk { get; set; }
        public Nullable<short> seat { get; set; }
        public Nullable<bool> available { get; set; }
        public Nullable<bool> anyone { get; set; }
    
        public virtual tb_room tb_room { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_seat_student> tb_seat_student { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tb_order> tb_order { get; set; }
    }
}
