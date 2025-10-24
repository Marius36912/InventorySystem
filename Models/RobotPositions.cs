namespace InventorySystem2.Models;

public static class RobotPositions
{
    // Lagerbokse i et 10 cm grid (meter). S flyttes lidt tættere på (0.25 i stedet for 0.30)
    public static readonly (double x, double y) A = (0.10, 0.00);
    public static readonly (double x, double y) B = (0.20, 0.00);
    public static readonly (double x, double y) C = (0.30, 0.00);
    public static readonly (double x, double y) S = (0.20, 0.25);

    // Lidt højere Z for at undgå grænsetilfælde: 0.20 m
    private const double Z  = 0.20;
    private const double RX = 0.0, RY = -3.1415, RZ = 0.0;

    public static string GenerateMove((double x, double y) from, (double x, double y) to)
        => $@"
  # Sikker home først (stabil IK-udgangspunkt)
  home = [0, -1.57, 0, -1.57, 0, 0]
  movej(home, a=1.2, v=0.6)

  # Poses i verden (x,y,Z) med stabil orientering
  p_from = p[{from.x}, {from.y}, {Z}, {RX}, {RY}, {RZ}]
  p_to   = p[{to.x},   {to.y},   {Z}, {RX}, {RY}, {RZ}]

  # To simple hop
  movej(get_inverse_kin(p_from), a=1.2, v=0.6)
  movej(get_inverse_kin(p_to),   a=1.2, v=0.6)
";
}