using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.Model.Ghn
{
    public class GhnResponse<T>
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }
    }

    public class GhnProvinceDto
    {
        [JsonPropertyName("ProvinceID")]
        public int ProvinceId { get; set; }

        [JsonPropertyName("ProvinceName")]
        public string ProvinceName { get; set; }

        [JsonPropertyName("Code")]
        public string Code { get; set; }
    }

    public class GhnDistrictDto
    {
        [JsonPropertyName("DistrictID")]
        public int DistrictId { get; set; }

        [JsonPropertyName("ProvinceID")]
        public int ProvinceId { get; set; }

        [JsonPropertyName("DistrictName")]
        public string DistrictName { get; set; }

        [JsonPropertyName("Code")]
        public string Code { get; set; }
    }

    public class GhnWardDto
    {
        [JsonPropertyName("WardCode")]
        public string WardCode { get; set; }

        [JsonPropertyName("DistrictID")]
        public int DistrictId { get; set; }

        [JsonPropertyName("WardName")]
        public string WardName { get; set; }
    }

    public class GhnFeeRequestDto
    {
        public int ToDistrictId { get; set; }
        public string ToWardCode { get; set; }
        public int Weight { get; set; } = 500; // Grams
        public int Length { get; set; } = 20;  // CM
        public int Width { get; set; } = 15;   // CM
        public int Height { get; set; } = 10;  // CM
    }

    public class GhnFeeDataDto
    {
        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("service_fee")]
        public decimal ServiceFee { get; set; }
    }

    public class GhnCreateOrderRequest
    {
        [JsonPropertyName("client_order_code")]
        public string? ClientOrderCode { get; set; }

        [JsonPropertyName("payment_type_id")]
        public int PaymentTypeId { get; set; } = 2; // 1: Sender, 2: Receiver

        [JsonPropertyName("note")]
        public string Note { get; set; } = "Cua hang do choi";

        [JsonPropertyName("required_note")]
        public string RequiredNote { get; set; } = "KHONGCHOXEMHANG";

        [JsonPropertyName("from_name")]
        public string FromName { get; set; } = "ToysWorld";

        [JsonPropertyName("from_phone")]
        public string FromPhone { get; set; } = "0387261918";

        [JsonPropertyName("from_address")]
        public string FromAddress { get; set; } = "Phuong Minh Khai, Quan Bac Tu Liem, Ha Noi";

        [JsonPropertyName("from_ward_code")]
        public string FromWardCode { get; set; } = "11013";

        [JsonPropertyName("from_district_id")]
        public int FromDistrictId { get; set; } = 1482;

        [JsonPropertyName("to_name")]
        public string ToName { get; set; }

        [JsonPropertyName("to_phone")]
        public string ToPhone { get; set; }

        [JsonPropertyName("to_address")]
        public string ToAddress { get; set; }

        [JsonPropertyName("to_ward_code")]
        public string ToWardCode { get; set; }

        [JsonPropertyName("to_district_id")]
        public int ToDistrictId { get; set; }

        [JsonPropertyName("cod_amount")]
        public int CodAmount { get; set; } = 0;

        [JsonPropertyName("content")]
        public string Content { get; set; } = "Do choi tre em";

        [JsonPropertyName("weight")]
        public int Weight { get; set; } = 500;

        [JsonPropertyName("length")]
        public int Length { get; set; } = 20;

        [JsonPropertyName("width")]
        public int Width { get; set; } = 15;

        [JsonPropertyName("height")]
        public int Height { get; set; } = 10;

        [JsonPropertyName("service_type_id")]
        public int ServiceTypeId { get; set; } = 2;

        [JsonPropertyName("items")]
        public List<GhnOrderItem> Items { get; set; } = new List<GhnOrderItem>();
    }

    public class GhnOrderItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }
    }

    public class GhnCreateOrderData
    {
        [JsonPropertyName("order_code")]
        public string OrderCode { get; set; }

        [JsonPropertyName("total_fee")]
        public decimal TotalFee { get; set; }

        [JsonPropertyName("expected_delivery_time")]
        public string ExpectedDeliveryTime { get; set; }
    }

    public class GhnWebhookDto
    {
        [JsonPropertyName("ShopID")]
        public int ShopID { get; set; }

        [JsonPropertyName("OrderCode")]
        public string OrderCode { get; set; }

        [JsonPropertyName("ClientOrderCode")]
        public string ClientOrderCode { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; }

        [JsonPropertyName("Type")]
        public string Type { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }

        [JsonPropertyName("Reason")]
        public string Reason { get; set; }

        [JsonPropertyName("ReasonCode")]
        public string ReasonCode { get; set; }

        [JsonPropertyName("Time")]
        public object Time { get; set; }

        [JsonPropertyName("CODAmount")]
        public decimal CODAmount { get; set; }

        [JsonPropertyName("TotalFee")]
        public decimal TotalFee { get; set; }

        [JsonPropertyName("Warehouse")]
        public string Warehouse { get; set; }

        [JsonPropertyName("ShipperName")]
        public string ShipperName { get; set; }

        [JsonPropertyName("ShipperPhone")]
        public string ShipperPhone { get; set; }

        [JsonPropertyName("PodURL")]
        public string PodURL { get; set; }
    }
}
