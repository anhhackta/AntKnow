using System;
using System.Collections.Generic;

/// <summary>
/// Event Card Handler - Xử lý các loại event card
/// Sử dụng EventCardLibrary (local Unity, không Firebase)
/// </summary>
public class EventCardHandler
{
    private readonly Random _rng;
    
    public EventCardHandler(Random rng = null)
    {
        _rng = rng ?? new Random();
    }

    /// <summary>
    /// Draw random event card
    /// </summary>
    public EventCardLibrary.EventCard DrawEventCard()
    {
        return EventCardLibrary.GetRandomEventCard(_rng);
    }

    /// <summary>
    /// Execute event card effect
    /// </summary>
    public EventExecutionResult ExecuteEventCard(EventCardLibrary.EventCard card, PlayerState player, GameState gameState)
    {
        var result = new EventExecutionResult
        {
            success = false,
            cardName = card.name,
            description = card.description
        };

        try
        {
            switch (card.type)
            {
                case EventCardLibrary.EventCardType.GainMoney:
                    player.Money += card.value;
                    result.success = true;
                    result.message = $"Nhận {card.value} AntCoin";
                    result.moneyChange = card.value;
                    break;

                case EventCardLibrary.EventCardType.LoseMoney:
                    int lost = Math.Min(player.Money, card.value);
                    player.Money -= lost;
                    result.success = true;
                    result.message = $"Mất {lost} AntCoin";
                    result.moneyChange = -lost;
                    break;

                case EventCardLibrary.EventCardType.MoveForward:
                    int oldPos = player.NodeIndex;
                    player.NodeIndex = (player.NodeIndex + card.value) % gameState.BoardLength;
                    result.success = true;
                    result.message = $"Di chuyển tiến {card.value} ô (từ {oldPos} → {player.NodeIndex})";
                    result.positionChange = card.value;
                    break;

                case EventCardLibrary.EventCardType.MoveBackward:
                    int oldPos2 = player.NodeIndex;
                    player.NodeIndex = (player.NodeIndex - card.value + gameState.BoardLength) % gameState.BoardLength;
                    result.success = true;
                    result.message = $"Di chuyển lùi {card.value} ô (từ {oldPos2} → {player.NodeIndex})";
                    result.positionChange = -card.value;
                    break;

                case EventCardLibrary.EventCardType.GoToTile:
                    int oldPos3 = player.NodeIndex;
                    player.NodeIndex = card.value;
                    result.success = true;
                    result.message = $"Đi đến ô {card.value}";
                    result.positionChange = card.value - oldPos3;
                    break;

                case EventCardLibrary.EventCardType.PayToPlayers:
                    int totalPaid = 0;
                    foreach (var otherPlayer in gameState.Players)
                    {
                        if (otherPlayer.Id != player.Id)
                        {
                            int amount = Math.Min(player.Money, card.value);
                            player.Money -= amount;
                            otherPlayer.Money += amount;
                            totalPaid += amount;
                        }
                    }
                    result.success = true;
                    result.message = $"Trả {totalPaid} AntCoin cho các người chơi";
                    result.moneyChange = -totalPaid;
                    break;

                case EventCardLibrary.EventCardType.CollectFromPlayers:
                    int totalCollected = 0;
                    foreach (var otherPlayer in gameState.Players)
                    {
                        if (otherPlayer.Id != player.Id)
                        {
                            int amount = Math.Min(otherPlayer.Money, card.value);
                            otherPlayer.Money -= amount;
                            player.Money += amount;
                            totalCollected += amount;
                        }
                    }
                    result.success = true;
                    result.message = $"Thu {totalCollected} AntCoin từ các người chơi";
                    result.moneyChange = totalCollected;
                    break;

                case EventCardLibrary.EventCardType.RepairProperties:
                    int repairCost = 0;
                    foreach (int tileId in player.Owned)
                    {
                        if (gameState.Properties.ContainsKey(tileId))
                        {
                            var prop = gameState.Properties[tileId];
                            if (prop.HasHotel)
                            {
                                repairCost += card.value * 2; // Hotel = 2x house cost
                            }
                            else
                            {
                                repairCost += card.value * prop.Level;
                            }
                        }
                    }
                    player.Money -= repairCost;
                    if (player.Money < 0) player.Money = 0;
                    result.success = true;
                    result.message = $"Sửa chữa nhà: Trả {repairCost} AntCoin";
                    result.moneyChange = -repairCost;
                    break;

                case EventCardLibrary.EventCardType.GoToJail:
                    player.JailTurns = card.value;
                    player.NodeIndex = 10; // Jail tile
                    result.success = true;
                    result.message = $"Đi tù {card.value} lượt!";
                    result.jailTurns = card.value;
                    break;

                case EventCardLibrary.EventCardType.GetOutOfJailFree:
                    // Store in player cooldown (convention: key = -9999)
                    player.PassiveCooldown[-9999] = 1;
                    result.success = true;
                    result.message = "Nhận thẻ ra tù miễn phí!";
                    break;

                case EventCardLibrary.EventCardType.TaxPerProperty:
                    int tax = player.Owned.Count * card.value;
                    player.Money -= tax;
                    if (player.Money < 0) player.Money = 0;
                    result.success = true;
                    result.message = $"Thuế tài sản: Trả {tax} AntCoin ({player.Owned.Count} ô × {card.value})";
                    result.moneyChange = -tax;
                    break;

                default:
                    result.success = false;
                    result.message = $"Event type {card.type} chưa được implement";
                    break;
            }
        }
        catch (Exception ex)
        {
            result.success = false;
            result.message = $"Lỗi khi thực hiện event: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// Check if player can afford event (before executing)
    /// </summary>
    public bool CanAffordEvent(EventCardLibrary.EventCard card, PlayerState player, GameState gameState)
    {
        switch (card.type)
        {
            case EventCardLibrary.EventCardType.LoseMoney:
                return player.Money >= card.value;
            
            case EventCardLibrary.EventCardType.PayToPlayers:
                return player.Money >= card.value * (gameState.Players.Count - 1);
            
            case EventCardLibrary.EventCardType.RepairProperties:
                // Calculate repair cost
                int repairCost = 0;
                foreach (int tileId in player.Owned)
                {
                    if (gameState.Properties.ContainsKey(tileId))
                    {
                        var prop = gameState.Properties[tileId];
                        if (prop.HasHotel)
                            repairCost += card.value * 2;
                        else
                            repairCost += card.value * prop.Level;
                    }
                }
                return player.Money >= repairCost;
            
            case EventCardLibrary.EventCardType.TaxPerProperty:
                return player.Money >= player.Owned.Count * card.value;
            
            default:
                return true; // Other events don't require money
        }
    }
}

/// <summary>
/// Event execution result
/// </summary>
public class EventExecutionResult
{
    public bool success;
    public string cardName;
    public string description;
    public string message;
    public int moneyChange;
    public int positionChange;
    public int jailTurns;
}

