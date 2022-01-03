Vector = {}
Vector.__index = Vector

local function newVector( x, y, z )
    return setmetatable( { x = x or 0, y = y or 0, z = z or 0 }, Vector )
end

function isvector( vTbl )
    return getmetatable( vTbl ) == Vector
end

function Vector.__unm( vTbl )
    return newVector( -vTbl.x, -vTbl.y, -vTbl.z )
end

function Vector.__add( a, b )
    return newVector( a.x + b.x, a.y + b.y, a.z + b.z )
end

function Vector.__sub( a, b )
    return newVector( a.x - b.x, a.y - b.y, a.z - b.z )
end

function Vector.__mul( a, b )
    if type( a ) == "number" then
        return newVector( a * b.x, a * b.y, a * b.z )
    elseif type( b ) == "number" then
        return newVector( a.x * b, a.y * b, a.z * b )
    else
        return newVector( a.x * b.x, a.y * b.y, a.z * b.z )
    end
end

function Vector.__div( a, b )
    return newVector( a.x / b, a.y / b, a.z / b )
end

function Vector.__eq( a, b )
    return a.x == b.x and a.y == b.y and a.z == b.z
end

function Vector:__tostring()
    return "(" .. self.x .. ", " .. self.y .. ", " .. self.z .. ")"
end

return setmetatable( Vector, { __call = function( _, ... ) return newVector( ... ) end } )