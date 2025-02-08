namespace comment.Data.Model
{
    public class CommentQueueItem
    {
        public Comment Comment { get; set; }
        public List<(byte[] FileData, string FileName)> Files { get; set; } // Измените тип на байтовые данные и имя файла
    }

}
