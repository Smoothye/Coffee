namespace WeddingPlannerApp.Components.PagesPaula.MenuComponents;

public static class MenuFallbackPackages
{
    public static List<MenuPackage> Create() =>
    [
        new("standard-classic", "Classic Elegance", "standard", 85, "A timeless menu featuring premium meats, seasonal sides and indulgent desserts.",
        [
            Course("Starters",
                Item("Bruschetta al Pomodoro", "Toasted sourdough, heirloom tomatoes, basil, aged balsamic", "vegetarian"),
                Item("Prawn Cocktail", "Tiger prawns, Marie Rose sauce, lemon, baby gem lettuce", "gf"),
                Item("Cream of Mushroom Soup", "Wild mushroom veloute, truffle oil, chive cream", "vegetarian", "gf")),
            Course("Main Course",
                Item("Herb-Crusted Chicken", "Free-range breast, roasted garlic jus, fondant potato", "gf"),
                Item("Slow-Roasted Beef Sirloin", "28-day aged sirloin, red wine reduction, dauphinoise potato", "gf"),
                Item("Pan-Seared Salmon", "Atlantic salmon, lemon butter, asparagus, new potatoes", "gf")),
            Course("Desserts",
                Item("Wedding Cake Slice", "Three-tier vanilla sponge, Swiss meringue buttercream", "vegetarian"),
                Item("Creme Brulee", "Classic vanilla custard, caramelized sugar, fresh berries", "vegetarian", "gf"),
                Item("Chocolate Fondant", "Warm dark chocolate cake, vanilla bean ice cream", "vegetarian"))
        ]),

        new("standard-premium", "Premium Feast", "standard", 125, "An elevated experience with premium cuts, seafood and artisan desserts.",
        [
            Course("Starters",
                Item("Lobster Bisque", "Creamy lobster soup, creme fraiche, chives, cognac", "gf"),
                Item("Beef Carpaccio", "Thinly sliced tenderloin, rocket, parmesan, capers", "gf"),
                Item("Burrata & Heirloom Tomato", "Fresh burrata, heritage tomatoes, basil oil, sea salt", "vegetarian", "gf")),
            Course("Main Course",
                Item("Filet Mignon", "8oz beef tenderloin, truffle butter, pomme puree", "gf"),
                Item("Butter-Poached Lobster", "Whole lobster tail, drawn butter, tarragon, asparagus", "gf"),
                Item("Duck Confit", "Slow-cooked duck leg, cherry jus, roasted root vegetables", "gf")),
            Course("Desserts",
                Item("Petit Fours Tower", "Macarons, chocolate truffles, miniature fruit tarts", "vegetarian"),
                Item("Champagne Panna Cotta", "Prosecco-set cream, raspberry coulis, gold leaf", "vegetarian", "gf"))
        ]),

        new("veg-garden", "Garden Fresh", "vegetarian", 75, "Vibrant vegetarian items showcasing seasonal produce and bold Mediterranean flavours.",
        [
            Course("Starters",
                Item("Caprese Salad", "Burrata, heirloom tomatoes, basil pesto, pine nuts", "vegetarian", "gf"),
                Item("Roasted Red Pepper Soup", "Charred peppers, smoked paprika, creme fraiche", "vegetarian", "gf"),
                Item("Cheese & Chutney Board", "Artisan cheeses, honeycomb, walnuts, crackers", "vegetarian")),
            Course("Main Course",
                Item("Wild Mushroom Risotto", "Arborio rice, porcini, parmesan, truffle oil", "vegetarian", "gf"),
                Item("Spinach & Ricotta Cannelloni", "Fresh pasta, tomato coulis, parmesan gratin", "vegetarian"),
                Item("Mediterranean Tart", "Roasted vegetables, feta, olive tapenade, shortcrust pastry", "vegetarian")),
            Course("Desserts",
                Item("Lemon Posset", "Set lemon cream, shortbread, mixed berry compote", "vegetarian", "gf"),
                Item("Tiramisu", "Mascarpone, espresso-soaked sponge, cocoa dusting", "vegetarian"))
        ]),

        new("veg-premium", "Vegetarian Gala", "vegetarian", 95, "Elegant fine-dining vegetarian experience with refined French and Italian influences.",
        [
            Course("Starters",
                Item("Goat Cheese Mousse", "Whipped chevre, candied walnuts, beet gel, watercress", "vegetarian", "gf"),
                Item("Truffle Arancini", "Crispy risotto balls, truffle aioli, aged parmesan", "vegetarian"),
                Item("Gazpacho Shot", "Chilled heirloom tomato, cucumber, green oil", "vegan", "gf")),
            Course("Main Course",
                Item("Mushroom Wellington", "King oyster & portobello, spinach, puff pastry, red wine jus", "vegetarian"),
                Item("Black Truffle Pasta", "Fresh pappardelle, black truffle, pecorino, brown butter", "vegetarian"),
                Item("Cauliflower Royale", "Whole roasted cauliflower, almond cream, herb gremolata", "vegetarian", "gf")),
            Course("Desserts",
                Item("Chocolate & Hazelnut Tart", "Dark chocolate ganache, praline, gold leaf", "vegetarian"),
                Item("Creme Caramel", "Classic French custard, caramel sauce, almond tuile", "vegetarian", "gf"))
        ]),

        new("vegan-light", "Plant Forward", "vegan", 70, "Colourful whole-food plant-based menu. 100% vegan, every item gluten-free friendly.",
        [
            Course("Starters",
                Item("Roasted Beet Carpaccio", "Golden beets, cashew cream, microgreens, walnut vinaigrette", "vegan", "gf"),
                Item("Gazpacho", "Chilled tomato, cucumber, peppers, sourdough croutons", "vegan"),
                Item("Guacamole & Crudites", "Fresh avocado, lime, chilli, seasonal vegetable sticks", "vegan", "gf")),
            Course("Main Course",
                Item("Lentil & Butternut Tagine", "Moroccan spices, chickpeas, preserved lemon, couscous", "vegan"),
                Item("Grilled Vegetable Stack", "Courgette, aubergine, peppers, quinoa, tahini", "vegan", "gf"),
                Item("Jackfruit Wellington", "BBQ jackfruit, mushroom duxelles, puff pastry, gravy", "vegan")),
            Course("Desserts",
                Item("Coconut Panna Cotta", "Coconut cream, passion fruit coulis, toasted coconut", "vegan", "gf"),
                Item("Chocolate Avocado Mousse", "Dark chocolate, avocado, maple, vanilla, berry compote", "vegan", "gf"))
        ]),

        new("vegan-premium", "Vegan Luxe", "vegan", 95, "Sophisticated plant-based fine dining that surprises even the most dedicated omnivore.",
        [
            Course("Starters",
                Item("Smoked Carrot Lox", "Cured carrot, capers, cream cheese (vegan), rye crispbread", "vegan"),
                Item("Avocado Tartare", "Diced avocado, mango salsa, lime dressing, sesame", "vegan", "gf"),
                Item("White Truffle Bisque", "Celeriac veloute, truffle oil, crispy sage", "vegan", "gf")),
            Course("Main Course",
                Item("Beetroot Wellington", "Roasted beet, walnut filling, puff pastry, jus", "vegan"),
                Item("Saffron Cauliflower Steak", "Whole roasted cauliflower, chimichurri, almond romesco", "vegan", "gf"),
                Item("Truffle Risotto", "Arborio, black truffle, cashew parmesan, herb oil", "vegan", "gf")),
            Course("Desserts",
                Item("Raw Chocolate Torte", "Dates, cacao, cashew cream, raspberry, gold dust", "vegan", "gf"),
                Item("Mango Sorbet Trio", "Alphonso mango, passion fruit, raspberry - three quenelles", "vegan", "gf"))
        ]),

        new("fasting-traditional", "Fasting Celebration", "fasting", 68, "A full fasting-friendly menu without meat, dairy or eggs, built around warm traditional items.",
        [
            Course("Starters",
                Item("Eggplant Salad Cups", "Roasted eggplant spread, tomatoes, parsley, toasted bread", "vegan", "fasting"),
                Item("Bean Pate Crostini", "Creamy white beans, caramelized onion, pickles", "vegan", "fasting"),
                Item("Stuffed Peppers", "Rice, mushrooms, herbs and tomato sauce", "vegan", "gf", "fasting")),
            Course("Main Course",
                Item("Mushroom Sarmale", "Cabbage rolls with mushroom rice filling and polenta", "vegan", "gf", "fasting"),
                Item("Vegetable Pilaf", "Seasonal vegetables, rice, herbs and roasted pepper", "vegan", "gf", "fasting"),
                Item("Grilled Vegetable Platter", "Courgette, aubergine, peppers, potatoes and garlic oil", "vegan", "gf", "fasting")),
            Course("Desserts",
                Item("Apple Strudel", "Spiced apples, raisins and crisp pastry", "vegan", "fasting"),
                Item("Dark Chocolate Fruit Cup", "Seasonal fruit, dark chocolate and toasted walnuts", "vegan", "gf", "fasting"))
        ]),

        new("fasting-premium", "Fasting Feast", "fasting", 88, "A more refined fasting menu with layered vegetable courses, richer sauces and elegant plated desserts.",
        [
            Course("Starters",
                Item("Marinated Mushroom Salad", "Forest mushrooms, garlic oil, dill and pickled onion", "vegan", "gf", "fasting"),
                Item("Roasted Pepper Rolls", "Sweet peppers filled with walnut cream and herbs", "vegan", "gf", "fasting"),
                Item("Cauliflower Cream Soup", "Silky cauliflower, crispy leeks and paprika oil", "vegan", "gf", "fasting")),
            Course("Main Course",
                Item("Walnut Mushroom Steak", "Pressed mushroom and walnut roast, potato puree, jus", "vegan", "fasting"),
                Item("Fasting Stuffed Cabbage", "Rice, mushrooms, smoked paprika and tomato sauce", "vegan", "gf", "fasting"),
                Item("Herbed Polenta Plate", "Creamy polenta, roasted vegetables, garlic spinach", "vegan", "gf", "fasting")),
            Course("Desserts",
                Item("Pear & Walnut Tart", "Caramelized pears, walnuts and cinnamon syrup", "vegan", "fasting"),
                Item("Cocoa Cherry Cake", "Dark cocoa sponge, sour cherries and vanilla glaze", "vegan", "fasting"))
        ])
    ];

    static Course Course(string label, params MenuItem[] items) => new(label, items.ToList());

    static MenuItem Item(string name, string description, params string[] dietaryTags) =>
        new(name, description, dietaryTags.ToList());
}
