using System;

// Tile types for board game
// MUST MATCH CLIENT Domain/Enums.cs EXACTLY!
public enum TileType {
    Start,          // Ô 0: Ô Bắt Đầu (Start) - +200 salary when pass
    Property,       // 26 ô: Các thành phố (có thể mua)
    Tax,            // Ô thuế (trừ tiền) - KHÔNG DÙNG trong map 36
    Bonus,          // Ô thưởng (cộng tiền) - KHÔNG DÙNG trong map 36
    Chance,         // Ô 7, 16, 25, 33: Ô Event (rút thẻ event) - Client gọi là "Event"
    Accident,       // KHÔNG DÙNG - Client dùng "Jail" cho ô tai nạn
    Quiz,           // Ô 19: Ô Tra Khảo (Quiz)
    Travel,         // Ô 28: Ô Du Lịch (Travel)
    Jail,           // Ô 10: Ô Tai Nạn (Jail/Accident) - Bị giam 3 turns
    GoToJail,       // Ô đi tù (bắt vào tù) - KHÔNG DÙNG trong map 36
    FreeParking     // Ô đỗ xe miễn phí - KHÔNG DÙNG trong map 36
}

// NOTE: Map 36 tiles thực tế chỉ dùng:
// - Start (ô 0)
// - Property (26 ô: cities)
// - Chance (4 ô: 7, 16, 25, 33) - Client gọi là "Event"
// - Quiz (ô 19)
// - Jail (ô 10) - Client gọi là "Accident"
// - Travel (ô 28)

// Property owner (player ID)
public enum Owner { 
    None = 0,   // Chưa có chủ
    P1 = 1,     // Player 1
    P2 = 2,     // Player 2
    P3 = 3,     // Player 3
    P4 = 4      // Player 4
}

// Card types
public enum CardType { 
    Passive,    // Kỹ năng thụ động (tự động kích hoạt)
    Active      // Kỹ năng chủ động (người chơi kích hoạt)
}

// Card trigger conditions
public enum CardTrigger {
    Manual,             // Kích hoạt thủ công
    StartOfTurn,        // Đầu lượt
    EndOfTurn,          // Cuối lượt
    OnQuizFail,         // Khi trả lời sai câu hỏi
    OnRentPay,          // Khi trả tiền thuê
    OnRentReceive,      // Khi nhận tiền thuê
    OnTravel,           // Khi di chuyển
    Custom              // Tùy chỉnh
}

