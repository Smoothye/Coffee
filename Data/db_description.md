# Database model  

## USER  
- user_id PK  
- user_role_id FK  
- first_name  
- last_name  
- email UNIQUE  
- password_hash
- created_at  
- updated_at  

## USER_ROLE  
- user_role_id PK  
- role_name UNIQUE  

## USER_EVENT  
- user_id PK/FK  
- event_id PK/FK  

Important: don't save the actual password, only a hashed version  

## VENUES  
- venue_id PK  
- name  
- address  
- min_capacity  
- max_capacity  
- estimated-price  

## EVENTS  
- event_id PK  
- venue_id FK  
- menu_id FK  
- name  
- event_date  
- estimated_guests  
- total_budget  
- notes  
- created_at  
- updated_at  

## GUEST  
- guest_id PK  
- event_id FK  
- table_id FK  
- rsvp_status_id FK  
- first_name  
- last_name  
- phone  
- email  
- category  
- age  
- gender  
- dietary_requirments  
- notes  
- created_at
- updated_at

## RSVP_STATUS  
- rsvp_status_id PK  
- status_name  

## MENU  
- menu_id PK  
- name  
- price  
- description  

## WEDDING_TABLE  
- table_id PK  
- event_id FK  
- table_number  
- capacity  
- notes  

## CHECK_LIST_TASK  
- task_id PK  
- event_id FK  
- status_id FK  
- title  
- category  
- due_date  
- priority  
- notes  

## CHECK_LIST_TASK_STATUS  
- status_id PK  
- status_name UNIQUE  

## EXPENSE  
- expense_id PK  
- event_id FK  
- supplier_id FK  
- payment_status_id FK  
- category  
- description  
- amount  
- expense_date  
- created_at  
- updated_at  

## PAYMENT_STATUS  
- payment_status_id PK  
- status_name UNIQUE  

## SUPPLIER  
- supplier_id PK  
- name  
- supplier_type  
- phone  
- email  
- base_price  
- notes  

## EVENT_SUPPLIER  
- event_id PK/FK  
- supplier_id PK/FK  
- collaboration_status  
- notes  

## SCHEDULE_ITEM  
- schedule_item_id PK  
- event_id FK  
- title  
- start_time  
- duration_minutes  
- location  
- description  

