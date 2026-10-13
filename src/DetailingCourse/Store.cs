using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
namespace DetailingCourse {
public class Store {
 public Data Data=new Data(); public string PathName; public bool Recovered;
 public Store(string path) {
  PathName=path; if(!File.Exists(path)) return;
  try { Data=Read(path); Validate(); }
  catch { if(!File.Exists(path+".bak")) throw new InvalidDataException("Не удалось прочитать данные. Проверьте файл и резервную копию.");
   Data=Read(path+".bak"); Validate(); Recovered=true; }
 }
 static Data Read(string path) { using(var stream=File.OpenRead(path))
  return (Data)new DataContractJsonSerializer(typeof(Data)).ReadObject(stream); }
 public void Validate() {
  if(Data==null||Data.Bookings==null||Data.Materials==null||Data.Usages==null) throw new InvalidDataException("Неверная структура данных.");
  if(Data.Bookings.Select(x=>x.Id).Distinct().Count()!=Data.Bookings.Count||
     Data.Materials.Select(x=>x.Id).Distinct().Count()!=Data.Materials.Count||
     Data.Usages.Select(x=>x.Operation).Distinct().Count()!=Data.Usages.Count) throw new InvalidDataException("Повторяющиеся идентификаторы.");
  foreach(var b in Data.Bookings) if(b.Id<=0||String.IsNullOrWhiteSpace(b.Client)||String.IsNullOrWhiteSpace(b.Master)||b.Minutes<=0||
   !new[]{"Запланировано","В работе","Завершено","Отменено"}.Contains(b.Status)) throw new InvalidDataException("Неверная запись.");
  foreach(var m in Data.Materials) if(m.Id<=0||String.IsNullOrWhiteSpace(m.Name)||m.Stock<0||m.Threshold<0) throw new InvalidDataException("Неверный остаток.");
  foreach(var u in Data.Usages) if(u.Quantity<=0||String.IsNullOrWhiteSpace(u.Operation)||
   !Data.Bookings.Any(b=>b.Id==u.BookingId)||!Data.Materials.Any(m=>m.Id==u.MaterialId)) throw new InvalidDataException("Неверная ссылка расхода.");
 }
 public void Save() {
  Validate(); string full=Path.GetFullPath(PathName); Directory.CreateDirectory(Path.GetDirectoryName(full));
  string temp=full+".tmp"; using(var stream=File.Create(temp)) new DataContractJsonSerializer(typeof(Data)).WriteObject(stream,Data);
  if(File.Exists(full)) {
   // A recovered backup must not be replaced with the corrupted original.
   if(Recovered) { File.Copy(temp,full,true); File.Delete(temp); Recovered=false; }
   else File.Replace(temp,full,full+".bak",true);
  } else File.Move(temp,full);
 }
 public Booking Booking(int id) { var b=Data.Bookings.FirstOrDefault(x=>x.Id==id); if(b==null) throw new ArgumentException("Запись не найдена."); return b; }
 public Material Material(int id) { var m=Data.Materials.FirstOrDefault(x=>x.Id==id); if(m==null) throw new ArgumentException("Материал не найден."); return m; }
 public static string Text(string s,string field) { if(String.IsNullOrWhiteSpace(s)) throw new ArgumentException("Заполните поле: "+field); return s.Trim(); }
}
}
