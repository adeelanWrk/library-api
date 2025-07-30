using System;
namespace Library.API.DTOs.History;

public class RequestParamHistoryDto
{
    public int authorId { get; set; }
    public int bookId { get; set; }
}