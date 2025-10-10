using System;
using System.Collections.Generic;

/// <summary>
/// Event Card Library - Lưu local trong Unity, không dùng Firebase
/// Event cards được random khi player vào ô Event (Chance)
/// </summary>
public static class EventCardLibrary
{
    /// <summary>
    /// Event card definition
    /// </summary>
    [Serializable]
    public class EventCard
    {
        public int id;
        public string name;
        public string description;
        public EventCardType type;
        public int value; // Money amount, steps, etc.
        public int weight; // Random weight (higher = more common)
    }

    public enum EventCardType
    {
        GainMoney,      // Nhận tiền
        LoseMoney,      // Mất tiền
        MoveForward,    // Di chuyển tiến
        MoveBackward,   // Di chuyển lùi
        GoToTile,       // Đi tới ô cụ thể
        PayToPlayers,   // Trả tiền cho tất cả người chơi
        CollectFromPlayers, // Thu tiền từ tất cả người chơi
        FreeProperty,   // Nhận 1 ô đất miễn phí
        RepairProperties, // Sửa chữa nhà (trả theo số nhà)
        GoToJail,       // Đi tù
        GetOutOfJailFree, // Thẻ ra tù miễn phí
        TaxPerProperty  // Thuế theo số ô đất sở hữu
    }

    /// <summary>
    /// Get all event cards
    /// </summary>
    public static List<EventCard> GetAllEventCards()
    {
        return new List<EventCard>
        {
            // === GAIN MONEY ===
            new EventCard { id = 1, name = "Trúng Số Độc Đắc", description = "Bạn trúng số! Nhận 500 AntCoin", type = EventCardType.GainMoney, value = 500, weight = 10 },
            new EventCard { id = 2, name = "Thưởng Tết", description = "Công ty thưởng Tết! Nhận 300 AntCoin", type = EventCardType.GainMoney, value = 300, weight = 15 },
            new EventCard { id = 3, name = "Bán Được Hàng", description = "Bạn bán được hàng! Nhận 200 AntCoin", type = EventCardType.GainMoney, value = 200, weight = 20 },
            new EventCard { id = 4, name = "Nhặt Được Tiền", description = "Bạn nhặt được ví! Nhận 150 AntCoin", type = EventCardType.GainMoney, value = 150, weight = 15 },

            // === LOSE MONEY ===
            new EventCard { id = 5, name = "Đóng Thuế Thu Nhập", description = "Phải đóng thuế! Mất 300 AntCoin", type = EventCardType.LoseMoney, value = 300, weight = 15 },
            new EventCard { id = 6, name = "Sửa Xe", description = "Xe hỏng cần sửa! Mất 150 AntCoin", type = EventCardType.LoseMoney, value = 150, weight = 20 },
            new EventCard { id = 7, name = "Đóng Tiền Điện", description = "Hóa đơn điện tháng này! Mất 100 AntCoin", type = EventCardType.LoseMoney, value = 100, weight = 20 },
            new EventCard { id = 8, name = "Mất Ví", description = "Bạn đánh mất ví! Mất 200 AntCoin", type = EventCardType.LoseMoney, value = 200, weight = 10 },

            // === MOVE FORWARD ===
            new EventCard { id = 9, name = "Đi Taxi", description = "Bắt taxi đi nhanh! Tiến 3 ô", type = EventCardType.MoveForward, value = 3, weight = 15 },
            new EventCard { id = 10, name = "Chạy Nhanh", description = "Bạn chạy được xa! Tiến 2 ô", type = EventCardType.MoveForward, value = 2, weight = 20 },
            new EventCard { id = 11, name = "Bay Nhanh", description = "Bay bằng máy bay! Tiến 5 ô", type = EventCardType.MoveForward, value = 5, weight = 10 },

            // === MOVE BACKWARD ===
            new EventCard { id = 12, name = "Đi Nhầm Đường", description = "Bạn đi nhầm đường! Lùi 2 ô", type = EventCardType.MoveBackward, value = 2, weight = 15 },
            new EventCard { id = 13, name = "Quên Đồ", description = "Quên đồ phải quay lại! Lùi 3 ô", type = EventCardType.MoveBackward, value = 3, weight = 10 },

            // === GO TO TILE ===
            new EventCard { id = 14, name = "Về Nhà", description = "Về ô Bắt Đầu", type = EventCardType.GoToTile, value = 0, weight = 10 },
            new EventCard { id = 15, name = "Đi Du Lịch", description = "Đi đến ô Du Lịch", type = EventCardType.GoToTile, value = 28, weight = 8 },

            // === PAY TO PLAYERS ===
            new EventCard { id = 16, name = "Sinh Nhật", description = "Hôm nay sinh nhật bạn! Mỗi người cho bạn 50 AntCoin", type = EventCardType.CollectFromPlayers, value = 50, weight = 10 },
            new EventCard { id = 17, name = "Tiệc Tất Niên", description = "Bạn tổ chức tiệc! Trả mỗi người 100 AntCoin", type = EventCardType.PayToPlayers, value = 100, weight = 10 },

            // === REPAIR PROPERTIES ===
            new EventCard { id = 18, name = "Sửa Nhà", description = "Sửa chữa tất cả nhà! Trả 50 AntCoin/nhà, 100 AntCoin/khách sạn", type = EventCardType.RepairProperties, value = 50, weight = 12 },

            // === GO TO JAIL ===
            new EventCard { id = 19, name = "Bị Bắt", description = "Bị bắt vì vi phạm giao thông! Đi tù 3 lượt", type = EventCardType.GoToJail, value = 3, weight = 8 },

            // === GET OUT OF JAIL FREE ===
            new EventCard { id = 20, name = "Thẻ Miễn Tù", description = "Giữ thẻ này để ra tù miễn phí 1 lần", type = EventCardType.GetOutOfJailFree, value = 1, weight = 5 },

            // === TAX PER PROPERTY ===
            new EventCard { id = 21, name = "Thuế Tài Sản", description = "Đóng thuế! Trả 50 AntCoin cho mỗi ô đất sở hữu", type = EventCardType.TaxPerProperty, value = 50, weight = 10 }
        };
    }

    /// <summary>
    /// Get random event card based on weight
    /// </summary>
    public static EventCard GetRandomEventCard(Random rng)
    {
        var cards = GetAllEventCards();
        int totalWeight = 0;
        foreach (var card in cards)
        {
            totalWeight += card.weight;
        }

        int roll = rng.Next(totalWeight);
        int cumulative = 0;

        foreach (var card in cards)
        {
            cumulative += card.weight;
            if (roll < cumulative)
            {
                return card;
            }
        }

        return cards[0]; // Fallback
    }

    /// <summary>
    /// Get event card by ID
    /// </summary>
    public static EventCard GetEventCardById(int id)
    {
        var cards = GetAllEventCards();
        return cards.Find(c => c.id == id);
    }
}

