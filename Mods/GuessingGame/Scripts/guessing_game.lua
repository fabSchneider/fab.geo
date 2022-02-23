-- A geo guessing game where the player has to give their best guess 
-- on the location of all the capitals of the world

-- Compatible with Fab.Geo version 0.4.0
-- Author: Fabian Schneider


function init()
    
    popup
        .button("Play 5", function() start_game(5) end)
        .button("Play 20", function() start_game(20) end)
        .button("Play 100", function() start_game(100) end)
        .button("Play all", function() start_game(-1) end)
        .show("Let's play!", "\nGuess the location of the world's capitals\n\nThe closer your guess the better!\n")
end

function load_capitals_data()
    local content = io.load("capitals.geojson")
    local data = json.parse(content)

    local capitals_data = data.features;
    local capitals = {}

    for _, c in next, capitals_data do     
        name = c.properties.city
        if name != nil and name != '' then
            coord = c.geometry.coordinates
            table.insert(capitals, 
            { 
                name = name, 
                country = c.properties.country,
                coord = Coord(coord[1], coord[2]) } )
        end
    end
    return capitals
end

game_state = nil

guess_pt = nil
target_pt = nil
off_line = nil

round_format = "Round %d of %d"

dots_color = Color(0.72, 0.12, 0.27, 1.0)
dots_fade_color = Color(0.72, 0.12, 0.27, 0.4)

function start_game(rounds)
    popup.close();

    controls.remove_all()

    local title = controls.label("title")
    title.bold = true
    title.center = true
    local text = controls.label("text")
    text.center = true
    
    controls.button("next", "Next", next_round)

    controls.hide()

    local capitals = load_capitals_data()

    if(rounds < 1) then
        rounds = #capitals
    else
        rounds = math.min(rounds, #capitals)
    end

    local objectives = {}

    for i = 1, rounds do
        local next = random.whole_number(1, #capitals + 1)
        local objective = table.remove(capitals, next) 
        table.insert(objectives, objective)
    end

    game_state = {
        objectives = objectives,
        round = 0,
        total_dist = 0,
        score = 0,
        guesses = {}
    }

    next_round()
end

function next_round()
    round = game_state.round
    round = round + 1

    if(guess_pt != nil) then
        guess_pt.name = ""
    end

    if(round > #game_state.objectives) then
        end_game()
        return
    end

    game_state.round = round

    controls.get("next").enabled = false

    controls.get("title").text = string.format(round_format,round, #game_state.objectives)
    controls.get("text").text = string.format("\n\n Find %s\n\n\n\n", game_state.objectives[round].name)
    controls.show()

    world.on_click(process_guess)
end

function end_game()

    controls.remove_all()
    controls.hide()

    local avg_dist = game_state.total_dist / #game_state.objectives

    local points = get_guess_answer(avg_dist);
    local stars = get_stars(points)

    features.remove_all()
    guess_pt = nil
    target_pt = nil
    off_line = nil      
    
    popup
        .button("Play 5", function() start_game(5) end)
        .button("Play 20", function() start_game(20) end)
        .button("Play 100", function() start_game(100) end)
        .button("Play all", function() start_game(-1) end)
        .show("Game finished", 
            string.format("\nYour score:\n\n%s\n\nOn average you were off by \n\n%dkm\n", 
            stars, avg_dist))
end

function process_guess(coord)

    world.on_click(nil)

    local round = game_state.round
    local target = game_state.objectives[round]
    local guess = coord
   
    if(round > 1) then
        guess_pt.color = dots_fade_color
        target_pt.color = dots_fade_color
        off_line.color = dots_fade_color
    end

    guess_pt = features.point("Your guess", coord)
    guess_pt.color = dots_color

    table.insert(game_state.guesses, coord)

    target_pt = features.point(target.name, target.coord)
    target_pt.color = dots_color
    off_line = features.line('off_line',  guess, target.coord)
    off_line.color = dots_color

    local distance = geo.distance(guess, target.coord)
    game_state.total_dist = game_state.total_dist + distance

    local points, answer = get_guess_answer(distance)
    game_state.score = game_state.score + points

    controls.get("text").text = string.format(
        "%s\n%s\n\nYour guess for %s\n%s\n\n was off by %dkm.\n", 
        get_stars(points), answer,  target.name, target.country, distance)
    controls.get("next").enabled = true
end

function get_stars(points)
    local stars = ""

    for i = 1, 5 do
        if(i <= points) then
            stars = stars .. ' '
        else
            stars = stars .. ' '
        end
    end
    return stars
end

function get_guess_answer(distance)
    local points = 0
    local answer = nil
    if distance < 50 then
        points = 5
        answer = "Wowsers, You got it!"
    elseif distance < 300 then
        points = 4
        answer = "Great! You got really close."
    elseif distance < 1400 then
        points = 3
        answer = "Not bad! You almost got it."
    elseif distance < 4000 then
        points = 2
        answer = "Hmmm, not quite."
    elseif distance < 8000 then
        points = 1
        answer = "Whoops, quite off!"
    else 
        points = 0
        answer = "Ouch, that's waaay off!"
    end

    return points, answer
end