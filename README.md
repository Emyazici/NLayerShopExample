# NLayerShopExample

Katmanlı mimari (N-Layer Architecture) kullanılarak geliştirilmiş örnek bir e-ticaret backend projesidir.  
Proje, sürdürülebilir ve genişletilebilir bir yapı kurmayı hedefler.

## Proje Amacı
- Katmanlı mimariyi gerçek bir API projesi üzerinde uygulamak
- İş kuralları ile veri erişimini birbirinden ayırmak
- Temiz ve okunabilir bir backend yapısı oluşturmak

## Kullanılan Teknolojiler
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Swagger (OpenAPI)

## Mimari Yapı

- **NLayerShopExample.Api**  
  API endpoint’leri, controller’lar, dependency injection ve uygulama konfigürasyonları.

- **NLayerShopExample.Business**  
  İş kuralları ve servis katmanı.

- **NLayerShopExample.Contracts**  
  DTO (Request / Response) modelleri ve katmanlar arası sözleşmeler.

- **NLayerShopExample.Data**  
  Entity Framework Core, DbContext, migration ve veri erişim katmanı.

- **NLayerShopExample.Domain**  
  Entity modelleri ve domain yapıları.

## Çalıştırma
1. Projeyi klonla
2. Veritabanını oluştur
3. API projesini çalıştır
4. Swagger arayüzü üzerinden endpoint’leri test et

## Projede Uygulanan Yaklaşımlar
- Katmanlı mimari
- Dependency Injection
- DTO kullanımı
- Clean code prensipleri

## Geliştirme Planı
- Global exception handling
- Validation mekanizması
- Authentication ve authorization
- Pagination ve filtering

## Lisans
MIT
