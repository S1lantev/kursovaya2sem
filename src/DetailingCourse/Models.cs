using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
namespace DetailingCourse {
[DataContract] public class Booking {
 [DataMember] public int Id; [DataMember] public string Client; [DataMember] public string Car;
 [DataMember] public string Service; [DataMember] public string Master; [DataMember] public DateTime Start;
 [DataMember] public int Minutes; [DataMember] public string Status="Запланировано";
}
[DataContract] public class Material {
 [DataMember] public int Id; [DataMember] public string Name; [DataMember] public decimal Stock;
 [DataMember] public decimal Threshold; [DataMember] public bool Reorder;
}
[DataContract] public class Usage {
 [DataMember] public string Operation; [DataMember] public int BookingId; [DataMember] public int MaterialId;
 [DataMember] public decimal Quantity;
}
[DataContract] public class Data {
 [DataMember] public List<Booking> Bookings=new List<Booking>();
 [DataMember] public List<Material> Materials=new List<Material>();
 [DataMember] public List<Usage> Usages=new List<Usage>();
}
public interface IModule { int Order {get;} string Name {get;} void Run(Store store); }
}
