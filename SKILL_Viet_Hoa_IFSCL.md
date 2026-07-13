# SKILL: Việt hóa file text game IFSCL (Code Lyoko fangame)

## 1. Bối cảnh dự án

IFSCL là một game mô phỏng/fangame lấy bối cảnh phim hoạt hình **Code Lyoko** (Pháp), xoay quanh nhóm bạn học sinh trường nội trú **Kadic Academy** khám phá ra một **siêu máy tính (supercomputer)** ẩn dưới một nhà máy bỏ hoang, kích hoạt một thế giới ảo tên là **Lyoko**, đánh thức một AI trí tuệ nhân tạo phản diện tên **XANA**, và giải cứu một cô gái ảo tên **Aelita** — con gái của nhà khoa học **Franz Hopper**.

Nhiệm vụ: Việt hóa toàn bộ text trong file CSV của game, đưa bản dịch vào cột `Vietnamese_Current`, đảm bảo:
- Hội thoại tự nhiên, đúng ngữ cảnh, đúng tính cách nhân vật.
- Text UI/hệ thống dịch chuẩn, ngắn gọn, nhất quán thuật ngữ.
- Hướng dẫn (tutorial/readme) dịch dễ hiểu nhưng **giữ nguyên các lệnh/phím tắt/cú pháp gameplay** không dịch.

---

## 2. Cấu trúc file dữ liệu

File CSV có (ít nhất) các cột:
- `ClassName`: nhóm nội dung (ví dụ `Ch01`, `Names`, `Titles`, `Readme_Cmd`...). File có khoảng **65 ClassName khác nhau**, tổng cộng **~10.052 dòng**.
- `ID`: định danh dòng thoại/text, thường theo mẫu `sceneTag_characterName_number` (ví dụ `meetHer_jeremie_003`). **ID là duy nhất trong phạm vi một ClassName** (đã kiểm chứng thực nghiệm không có ID trùng trong cùng một Ch/class) — nhưng có thể trùng giữa các class khác nhau, nên khi ghi kết quả luôn cần match theo **cặp (ClassName, ID)**, không match ID đơn lẻ.
- `English_Original`: text gốc tiếng Anh (nguồn dịch).
- `Vietnamese_Current`: cột cần điền bản dịch tiếng Việt (ban đầu trống toàn bộ).

Một số ô `English_Original` có thể trống hoặc là `-` (placeholder chưa có nội dung thật) → để `Vietnamese_Current` cũng trống/`-`, **không tự bịa nội dung**.

Text nhiều dòng dùng ký tự xuống dòng `\n` bên trong cùng một ô CSV — **giữ nguyên vị trí xuống dòng** khi dịch (không nhất thiết dịch từng dòng riêng biệt về nghĩa, nhưng cấu trúc ngắt dòng nên tương ứng hợp lý với bản gốc để không phá layout UI).

---

## 3. Danh sách các nhóm ClassName và loại nội dung

| Nhóm | Ví dụ ClassName | Loại nội dung | Ghi chú dịch |
|---|---|---|---|
| Cốt truyện chính | `Ch00`–`Ch17` | Hội thoại cutscene theo chương, đời sống học đường + phiêu lưu Lyoko | Dịch tự nhiên, đúng văn phong nhân vật (xem mục 5) |
| Cốt truyện phụ/mở-kết | `Ch50`–`Ch53`, `Ch60`, `Ch61`, `Ch90`, `Ch91`, `ChLost` | Cutscene đặc biệt, epilogue, nội dung ẩn | Tương tự Ch chính |
| Metadata chương | `ChNames`, `ChSynopsis` | Tên chương, tóm tắt chương | Dịch như tiêu đề/mô tả, súc tích |
| Hệ thống cảnh báo/log | `Alerts`, `Anomalies`, `States`, `PopupLx` | Thông báo trong game (XANA tấn công, tháp kích hoạt...) | Dịch UI, ngắn gọn, rõ nghĩa, giữ thuật ngữ nhất quán |
| Tên riêng & danh mục | `Names`, `Characters`, `Countries`, `Continents`, `ChNames` | Tên nhân vật, địa danh, quốc gia | **Tên riêng người/địa danh thật KHÔNG dịch** (ví dụ "Yumi", "France" → "Pháp" được vì là tên nước có tên tiếng Việt chuẩn, nhưng tên nhân vật giữ nguyên) |
| Từ vựng rời | `Words`, `Sentences` | Danh sách từ/câu đơn lẻ (có thể dùng để ghép câu động trong game) | Dịch sát nghĩa, đơn giản, không thêm ngữ cảnh không có |
| Tiêu đề/UI nhãn | `Titles`, `Tooltips`, `OptionNames`, `OptionDescr` | Tên chức năng, tooltip, tên tùy chọn cài đặt | Dịch UI chuẩn, thống nhất với các game/app tiếng Việt phổ biến (Cài đặt, Âm lượng, Độ khó...) |
| Chiến đấu | `BattleLx` | Text liên quan chiến đấu trên Lyoko (tên đòn, hiệu ứng...) | Giữ đúng thuật ngữ Lyoko (xem bảng thuật ngữ mục 4) |
| Nhật ký nhân vật | `DiaryAelita`, `DiaryFranz`, `DiaryJeremy`, `DiaryJim`, `DiaryKawa` | Trích đoạn nhật ký cá nhân | Văn phong viết nhật ký, ngôi thứ nhất, cảm xúc riêng tư — **xưng hô theo đúng nhân vật sở hữu nhật ký** (xem mục 5) |
| Vật phẩm & mục tiêu | `Inventory`, `Objectives`, `Photos`, `Sms`, `WebLx` | Tên/mô tả vật phẩm, nhiệm vụ, tin nhắn SMS trong game, trang web giả lập | Dịch tự nhiên như văn bản thật (SMS như tin nhắn thật, trang web như nội dung web thật) |
| Linh tinh xã hội | `BubbleThoughts`, `Common`, `SocialW`, `TvLx` | Bong bóng suy nghĩ NPC, câu nói chung, mạng xã hội giả lập, chương trình TV giả lập | Dịch đời thường, có thể hài hước/mỉa mai tùy ngữ cảnh gốc |
| Gợi ý & Holovision | `Hints`, `Holovis` | Gợi ý gameplay, nội dung máy chiếu ảnh 3D (Holomap) | Dịch rõ ràng, hướng dẫn dễ hiểu |
| Hướng dẫn (Readme) | `ReadmeStructAll`, `ReadmeStructBasics`, `Readme_Cmd`, `Readme_Color`, `Readme_Tooltip` | Tài liệu hướng dẫn chơi, giải thích cấu trúc, màu sắc, tooltip hệ thống | Dịch phần **giải thích/mô tả**, nhưng **KHÔNG dịch tên lệnh, phím tắt, cú pháp code, biến hệ thống** (xem mục 6) |

> Ghi chú: đây là bảng phân loại tổng quát dựa trên khảo sát 65 ClassName thực tế trong file mẫu 4.8.6. Khi làm việc với phiên bản file mới, cứ áp dụng logic phân loại tương tự cho ClassName lạ (dựa vào tên class + nội dung mẫu vài dòng đầu để đoán loại, rồi áp quy tắc phù hợp).

---

## 4. Bảng thuật ngữ cố định (PHẢI dùng nhất quán xuyên suốt toàn bộ file)

| Tiếng Anh | Tiếng Việt chuẩn dùng xuyên suốt |
|---|---|
| Lyoko | Lyoko (giữ nguyên, không dịch — là danh từ riêng/thế giới ảo) |
| XANA / Xana | XANA / Xana (giữ nguyên, không dịch) |
| supercomputer | siêu máy tính |
| scanner | máy quét |
| tower (Lyoko) | tháp |
| way tower | tháp trung chuyển |
| activated tower | tháp bị kích hoạt / tháp đang hoạt động |
| deactivate (tower) | vô hiệu hóa |
| virtualize / virtualization | ảo hóa |
| devirtualize / devirtualization | phi ảo hóa |
| materialize / materialization | vật chất hóa |
| return to the past (RTTP) | quay ngược thời gian |
| enhanced return to the past | quay ngược thời gian tăng cường / cỗ máy quay ngược thời gian tăng cường (tùy ngữ cảnh) |
| neural headset | mũ thần kinh |
| Holomap | Bản đồ ảnh ba chiều / Holomap (có thể giữ nguyên nếu ngữ cảnh là tên riêng tính năng UI) |
| Kolossus | Kolossus (giữ nguyên — tên quái vật/boss) |
| monster (chung) | quái vật |
| tower's core / core | lõi |
| Kadic Academy | trường Kadic (có thể giữ "Kadic Academy" trong văn bản trang trọng như thư mời, nhưng hội thoại thường dùng "trường Kadic") |
| pencak-silat | pencak-silat (giữ nguyên, tên môn võ thật) |
| the factory | nhà máy |
| the lab / laboratory | phòng thí nghiệm |
| Zombinator (tên phim giả tưởng trong game) | Zombinator (giữ nguyên, tên riêng sản phẩm hư cấu) |

**Tên nhân vật giữ nguyên không dịch:** Jeremy (Jeremie), Aelita, Odd, Ulrich, Yumi, William, Sissi, Herve/Herb, Nicolas, Milly, Tamiya, Jim/Jim Morales, Franz Hopper, Suzanne Hertz, Delmas, Yolande, Rosa, Kiwi (tên chó)...

---

## 5. Quy tắc xưng hô (BẮT BUỘC tuân thủ tuyệt đối, đây là điểm dễ sai nhất)

**Nguyên tắc chung: xưng hô dựa trên MỐI QUAN HỆ giữa hai nhân vật đang nói chuyện, không dựa trên tuổi tác thực của diễn viên lồng tiếng hay bối cảnh gốc tiếng Pháp/Anh.**

### 5.1. Giữa nhóm bạn học sinh với nhau — LUÔN dùng **cậu / tớ**
Áp dụng cho mọi cặp thoại giữa: Jeremy, Aelita, Odd, Ulrich, Yumi, William, và cả với các NPC học sinh khác (Sissi, Herve, Nicolas, Milly, Tamiya, Sandra, Thomas, Théo, Paul, Bastien, Mathieu, Tania, Noemie, Julien, Azra, Christophe, Heidi, Mathias...).

- **Áp dụng cả khi Jeremy và Aelita nói chuyện tình cảm/thân mật** (kể cả các đoạn về sau khi trưởng thành, ví dụ cảnh "Are you sure you're doing it for me... or for you?") — vẫn xưng **cậu/tớ**, KHÔNG dùng anh/em cho cặp đôi này.
- Không phân biệt giới tính hay vai trò lãng mạn — quy tắc cậu/tớ áp dụng đồng nhất cho toàn bộ nhóm bạn.

### 5.2. Học sinh nói chuyện với giáo viên/người lớn có vai trò giáo dục — học sinh xưng **em**
Áp dụng khi học sinh nói với: Jim Morales (giáo viên thể dục), Suzanne Hertz (giáo viên khoa học), giáo viên toán (Meyer), thầy hiệu trưởng Delmas, cô y tá Yolande, thầy dạy văn/sử Gilles, và các giáo viên/nhân viên trường khác.

- Học sinh: xưng "em", gọi người lớn là "thầy"/"cô" tương ứng.
- Giáo viên nói với học sinh: có thể xưng "tôi"/"cô"/"thầy" tùy độ thân mật, gọi học sinh bằng "em".

### 5.3. Quan hệ cha con: Franz Hopper ↔ Aelita — xưng **bố / con**
Đây là trường hợp đặc biệt vì là quan hệ huyết thống, không áp dụng quy tắc 5.1.

### 5.4. Franz Hopper (người lớn, không phải giáo viên chính thức) nói chuyện với nhóm bạn (không phải Aelita)
Dùng cặp **chú / cháu** (Franz là bạn của bố Aelita/nhà khoa học lớn tuổi, không phải thầy giáo ở trường, nên không dùng thầy/em, mà dùng chú/cháu mang tính gia đình/thân thiết hơn).

- Ví dụ: Jeremy nói với Franz → "Chú Franz...", Franz nói với Jeremy → gọi "cháu".

### 5.5. Trường hợp mơ hồ khác
- Nhân viên hành chính, phụ huynh, cảnh sát, người lạ trưởng thành nói với nhóm bạn (đang ở độ tuổi học sinh) → mặc định dùng **cháu/chú-cô** hoặc **em/anh-chị** tùy mức độ trang trọng của ngữ cảnh gốc; ưu tiên **cháu/chú-cô** cho người lớn tuổi hẳn (phụ huynh, cảnh sát), **em/anh-chị** nếu người nói trẻ hơn (sinh viên, nhân vật ở độ tuổi 20).
- Nếu không chắc chắn về tuổi/vai vế của một NPC mới xuất hiện, mặc định áp dụng như giáo viên (em/thầy-cô) nếu NPC đó ở trong khuôn viên trường, hoặc cậu/tớ nếu NPC đó rõ ràng là học sinh cùng trang lứa.

### 5.6. Xưng hô trong văn bản KHÔNG PHẢI hội thoại trực tiếp (nhật ký, mô tả, UI)
- Nhật ký (`DiaryX`): viết ở ngôi thứ nhất của chủ nhân nhật ký, tự nhiên như văn viết tay, không cần chèn "cậu/tớ" trừ khi nhật ký đó nhắc đến bạn bè — khi đó áp dụng đúng quy tắc 5.1/5.4 relative với người viết.
- Mô tả vật phẩm, tên tháp, tên kỹ năng (`Inventory`, `BattleLx`, `Titles`...): không có xưng hô, dịch như văn bản UI trung tính.

---

## 6. Nguyên tắc KHÔNG dịch (giữ nguyên để không phá gameplay)

Tuyệt đối giữ nguyên, không dịch, không thêm dấu tiếng Việt vào các thành phần sau dù chúng xuất hiện ở bất kỳ ClassName nào:

1. **Biến placeholder động** dạng `[PlayerName]`, `[XXX]`, `{0}`, `%s`, `<varname>` hoặc tương tự — đây là chỗ game sẽ tự động chèn tên người chơi/số liệu vào lúc chạy game.
2. **Thẻ định dạng HTML/markup** như `<i>...</i>`, `<b>...</b>`, `<color=...>...</color>` — giữ nguyên thẻ, chỉ dịch phần text bên trong.
3. **Tên phím tắt, lệnh, cú pháp code** xuất hiện trong các class `Readme_Cmd`, `Readme_Tooltip`, hoặc bất kỳ đâu có dạng lệnh điều khiển game (ví dụ tên phím WASD, tên nút bấm cụ thể của bàn phím/tay cầm, cú pháp lệnh console/debug nếu có). Chỉ dịch phần **mô tả/giải thích** đi kèm lệnh đó.
4. **Tên chương trình/thuật ngữ kỹ thuật riêng của game** nếu đã có quy ước giữ nguyên trong bảng thuật ngữ mục 4 (Lyoko, XANA, Kolossus...).
5. Ô có `English_Original` trống hoặc chỉ chứa ký tự placeholder như `-` → để `Vietnamese_Current` cũng trống/giữ `-`, không tự sáng tác nội dung.
6. Các ghi chú đạo diễn/mô tả hành động trong ngoặc đơn (kể cả khi gốc viết bằng tiếng Pháp xen lẫn, ví dụ `(il ressort ces plans du casque neuronal devant aelita)`) — **NÊN dịch sang tiếng Việt** vì đây là mô tả cảnh cho người đọc hiểu, không phải lệnh gameplay. (Ngoại lệ với mục 1-3 ở trên vẫn không dịch.)

---

## 7. Quy trình làm việc đề xuất (workflow từng bước)

Vì file rất lớn (~10.000 dòng, 65 nhóm), nên xử lý **tuần tự theo từng ClassName**, không cố dịch toàn bộ file cùng lúc để tránh sai sót và mất kiểm soát tiến độ. Với mỗi ClassName:

1. **Trích xuất**: Lọc toàn bộ dòng thuộc ClassName đó, lấy đúng thứ tự xuất hiện trong file gốc, xuất ra danh sách `(ID, English_Original)`.
2. **Kiểm tra ID trùng lặp** trong phạm vi ClassName đó (nếu có ID trùng, phải xử lý theo thứ tự dòng thay vì dùng ID làm khóa duy nhất).
3. **Dịch**: Với mỗi dòng, xác định:
   - Ai đang nói (dựa vào phần đầu của ID, ví dụ `jeremie_001`, `aelita_002`)?
   - Đang nói với ai (dựa vào ngữ cảnh đoạn hội thoại xung quanh)?
   - Áp dụng đúng quy tắc xưng hô ở mục 5.
   - Áp dụng đúng thuật ngữ ở mục 4.
   - Kiểm tra có phần nào thuộc diện "không dịch" ở mục 6 không.
4. **Đối chiếu đủ số lượng**: Sau khi dịch xong một ClassName, kiểm tra số lượng bản dịch phải khớp chính xác 100% với số dòng gốc trong ClassName đó (không thiếu, không thừa) trước khi ghi vào file.
5. **Ghi vào file**: Cập nhật cột `Vietnamese_Current` cho đúng từng dòng (match theo cặp `ClassName + ID`, hoặc theo đúng thứ tự dòng nếu ID trùng lặp trong class).
6. **Lưu file trung gian** sau mỗi ClassName hoàn thành (để không mất tiến độ nếu quá trình bị gián đoạn).
7. **Báo cáo tiến độ**: Sau mỗi đợt, tính tổng số dòng đã dịch / tổng số dòng toàn file, thông báo cho người dùng biết đã xong ClassName nào.
8. Lặp lại cho ClassName tiếp theo, theo thứ tự ưu tiên gợi ý ở mục 8.

---

## 8. Thứ tự ưu tiên xử lý các nhóm ClassName

1. **Cốt truyện chính**: `Ch01` → `Ch17` (theo số thứ tự tăng dần) — đây là phần quan trọng nhất, quyết định trải nghiệm chính của người chơi.
2. **Cốt truyện phụ/đặc biệt**: `Ch50`–`Ch53`, `Ch60`, `Ch61`, `Ch90`, `Ch91`, `ChLost`, `ChNames`, `ChSynopsis`.
3. **UI & hệ thống**: `Alerts`, `Anomalies`, `Titles`, `Tooltips`, `PopupLx`, `States`, `Objectives`, `Hints`, `OptionNames`, `OptionDescr`.
4. **Dữ liệu/tên riêng & từ vựng**: `Names`, `Characters`, `Countries`, `Continents`, `Words`, `Sentences`.
5. **Nhật ký & nội dung phụ**: `DiaryAelita`, `DiaryFranz`, `DiaryJeremy`, `DiaryJim`, `DiaryKawa`, `Inventory`, `Photos`, `Sms`, `WebLx`, `BubbleThoughts`, `Common`, `SocialW`, `BattleLx`, `TvLx`, `Holovis`.
6. **Hướng dẫn (Readme)**: `ReadmeStructAll`, `ReadmeStructBasics`, `Readme_Cmd`, `Readme_Color`, `Readme_Tooltip` — dịch phần giải thích, giữ nguyên lệnh/cú pháp theo mục 6.

---

## 9. Checklist tự kiểm tra trước khi hoàn thành mỗi đợt dịch

- [ ] Số dòng bản dịch khớp chính xác với số dòng gốc của ClassName đang xử lý.
- [ ] Không có ô `Vietnamese_Current` nào bị bỏ trống ngoại trừ những ô mà `English_Original` gốc cũng trống/placeholder.
- [ ] Tất cả xưng hô cậu/tớ – em/thầy-cô – bố/con – chú/cháu được áp dụng đúng theo mục 5 cho từng cặp nhân vật.
- [ ] Thuật ngữ (Lyoko, XANA, siêu máy tính, tháp, quay ngược thời gian...) dùng nhất quán, không đổi cách dịch giữa các dòng khác nhau cho cùng một khái niệm.
- [ ] Các biến động `[PlayerName]`, thẻ định dạng `<i>...</i>`, lệnh/phím tắt trong phần Readme được giữ nguyên, không bị dịch nhầm.
- [ ] Cấu trúc xuống dòng `\n` trong câu dài được giữ hợp lý, không làm vỡ bố cục UI gốc.
- [ ] Văn phong tự nhiên như người Việt thật sự nói chuyện — tránh dịch word-by-word cứng nhắc kiểu Google Translate (ví dụ không dịch "That's the spirit!" thành "Đó là tinh thần!" mà nên là "Tinh thần đó mới đúng chứ!").
- [ ] Các câu đùa/chơi chữ tiếng Anh được Việt hóa thành câu đùa tương đương có nghĩa trong tiếng Việt (không dịch sát nghĩa làm mất tính hài hước), miễn là không làm sai lệch nội dung cốt truyện.

---

## 10. Ví dụ mẫu đã dịch (tham khảo văn phong chuẩn cần đạt được)

```
EN: Good morning! Sorry for the beard…
VI: Chào buổi sáng con! Xin lỗi vì bộ râu này nhé…
(Franz nói với Aelita → bố/con)

EN: Aelita! How are you?
VI: Aelita! Em khỏe không?
(Cô giáo Suzanne nói với học sinh Aelita → giáo viên gọi "em")

EN: Oh. Good evening Suzanne.
VI: Ôi. Chào cô Suzanne ạ.
(Aelita nói với giáo viên → xưng "em", gọi "cô")

EN: Are you sure you're doing it for me... Or for you?
VI: Cậu có chắc là cậu làm điều này vì tớ... hay vì chính cậu không?
(Aelita nói với Jeremy, dù là cảnh tình cảm vẫn xưng cậu/tớ)

EN: Franz... How has XANA managed to do... all this?
VI: Chú Franz... làm sao XANA có thể làm được... tất cả chuyện này?
(Jeremy nói với Franz Hopper → chú/cháu)

EN: That's the spirit! Good dog!
VI: Đúng vậy! Tinh thần đó mới đúng chứ! Chó ngoan!
(dịch tự nhiên, không word-by-word)
```

---

## 11. Ghi chú thực thi kỹ thuật (dành cho AI xử lý bằng code/script)

- Đọc/ghi file CSV với encoding `utf-8-sig` để tương thích Excel khi mở lại trên Windows.
- Dùng `csv.DictReader` / `csv.DictWriter` (hoặc thư viện tương đương) để giữ nguyên toàn bộ cột khác không liên quan, chỉ cập nhật cột `Vietnamese_Current`.
- Khi ghi đè, luôn ghi ra file mới hoặc file làm việc trung gian trước, tránh làm hỏng file gốc nếu có lỗi giữa chừng.
- Nên xây dựng một dict tra cứu dạng `{(ClassName, ID): "bản dịch"}` cho mỗi đợt dịch, sau đó áp dụng vào toàn bộ rows — cách này tránh nhầm lẫn thứ tự dòng.
- Sau mỗi đợt, in ra số dòng đã cập nhật để đối chiếu với số dòng dự kiến (bắt buộc, không bỏ qua bước này).
- Giữ tổng tiến độ (đếm số ô `Vietnamese_Current` không rỗng / tổng số dòng) để báo cáo cho người dùng sau mỗi đợt.

---

## 12. Tình trạng bàn giao tại thời điểm viết skill này

Tính đến thời điểm viết tài liệu này, đã dịch xong và ghi vào file các nhóm sau (có thể dùng làm tham chiếu văn phong):
- `Ch50`, `Ch51`, `Ch52`, `Ch53` (72 dòng)
- `Ch00` (89 dòng)
- `Ch01` (230 dòng)
- `Ch02` (225 dòng)

Tổng cộng: **621/10.052 dòng** đã hoàn thành trong file `IFSCL_4_8_6_Toan_Bo_Text_Viet_Hoa.csv` đính kèm. AI tiếp theo nên tiếp tục từ `Ch03` trở đi, theo đúng thứ tự ưu tiên ở mục 8.
