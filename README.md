# Notification Microservices Platform

**Микросервисная система доставки уведомлений по Email, Push (боевые), SMS (заглушка). Полная инфраструктура для разработки, тестов и эксплуатации.**

---

## Состав

- **Notification.Api** — REST API для отправки уведомлений и запроса истории.
- **Notification.Worker.Email** — обработка и доставка Email (SMTP).
- **Notification.Worker.Push** — обработка и доставкой Push.
- **Notification.Worker.Sms** — обработка SMS (заглушка).
- **RabbitMQ** — брокер.
- **PostgreSQL** — хранилище сообщений и истории отправок.
- **MongoDB (опционально)** — поддержка событий/логов.
- **Prometheus & Grafana** — метрики.
- **ELK (Elasticsearch + Kibana + Filebeat)** — централизованные логи.

---

## Быстрый старт: локально

1. Требования:
    - Docker, Docker Compose
2. Клонировать репозиторий:
   ```bash
   git clone <ВАШ_URL>
   cd <ПАПКА_ПРОЕКТА>
   ```
3. Запуск всего стека:
   ```bash
   docker-compose up --build
   ```
4. Доступ:
   - API docs: http://localhost:5000/swagger
   - Grafana: http://localhost:3000 (admin/admin)
   - Prometheus: http://localhost:9090
   - Kibana: http://localhost:5601
   - RabbitMQ: http://localhost:15672 (guest/guest)
   - PostgreSQL: localhost:5432 (notif_user/notif_pass)

---

## Использование API

См. Swagger по адресу /swagger после запуска.

---

## Мониторинг и логи

- Метрики сервисов доступны на endpoint `/metrics`.
- Логи всех сервисов централизованы в Elasticsearch и доступны через Kibana.

---

## Kubernetes/Helm

- Kubernetes manifests и Helm chart: папка `k8s/` и `helm/`.
- Для развёртывания:
  ```bash
  kubectl apply -f k8s/
  # Или
  helm install notification-platform ./helm/
  ```
---

## Каналы и расширение

- Добавление нового канала: копируйте структуру любого worker и реализуйте способ доставки, подключив новую очередь RabbitMQ.
