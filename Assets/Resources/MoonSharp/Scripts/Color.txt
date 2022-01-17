Color = {}
Color.__index = Color

local function newColor( r, g, b, a )
    return setmetatable( { r = r or 0.0, g = g or 0.0, b = b or 0.0, a = a or 1.0 }, Color )
end

function iscolor( colorTbl )
    return getmetatable( colorTbl ) == Color
end

function Color.__eq( a, b )
    return a.r == b.r and a.g == b.g and a.b == b.b and a.a == a.a
end

function Color.red( )
    return newColor(1.0, 0.0, 0.0, 1.0)
end

function Color.green( )
    return newColor(0.0, 1.0, 0.0, 1.0)
end

function Color.blue( )
    return newColor(0.0, 0.0, 1.0, 1.0)
end

function Color.white( )
    return newColor(1.0, 1.0, 1.0, 1.0)
end

function Color.grey( )
    return newColor(0.5, 0.5, 0.5, 1.0)
end

function Color.black( )
    return newColor(0.0, 0.0, 0.0, 1.0)
end

function Color:__tostring()
    return "(r: " .. self.r .. ", g: " .. self.g .. ", b: " .. self.b .. ", a: " .. self.a .. ")"
end

return setmetatable( Color, { __call = function( _, ... ) return newColor( ... ) end } )