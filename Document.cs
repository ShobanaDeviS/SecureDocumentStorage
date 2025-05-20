public class Document
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string Owner { get; set; }
    public byte[] Content { get; set; }
    public int Revision { get; set; }
    public DateTime UploadedAt { get; set; }
}