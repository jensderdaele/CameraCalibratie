using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTK;

namespace Calibratie {
    class Matrix4dSerializer : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var mat = (Matrix4d) value;

            writer.WriteStartObject();

            writer.WriteStartArray();
            
            writer.WritePropertyName("rotation");
            writer.WriteValue(Matrix3d.CreateFromQuaternion(mat.ExtractRotation()));
            writer.WritePropertyName("position");
            writer.WriteValue(mat.ExtractTranslation());

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            //var product = existingValue as Matrix4d ?? new Matrix4d();
            var rot = Matrix3d.Identity;
            var trans = Vector3d.Zero;
            var mat = new Matrix4d();
            while (reader.Read()) {
                if (reader.TokenType == JsonToken.EndObject)
                    continue;
                /*
                var value = reader.Value.ToString();
                switch (value) {
                    case "rotation":
                        rot = reader.read
                        product.id = 
                        break;
                    case "position":
                        product.name = reader.ReadAsString();
                        break;
                }*/

            }
                
            return mat;
        }

        public override bool CanConvert(Type objectType) {
            return objectType == typeof(Matrix4d);
        }
    }
}
