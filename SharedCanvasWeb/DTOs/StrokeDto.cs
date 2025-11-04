namespace SharedCanvasWeb.DTOs;
public record StrokeDto(
    string Room,
    string Color,
    float Width,
    List<PointDto> Points,
    long CreatedAtTicks
);
