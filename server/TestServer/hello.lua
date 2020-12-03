function func(c)
	local map = c:map()
	print(string.format('map name : %s', map:name()))
	local width, height = map:size()
	print(string.format('map size : %s %s', width, height))


	local objects = map:objects()
	for key, value in pairs(objects) do
		local x, y = value:position()
		print(string.format('object position : %s %s', x, y))
	end

    print(string.format('hp : %s', c:hp()))
    print(string.format('damage : %s', c:damage()))

    local result = c:yield()
    print(string.format('return value : %s', result))

    print(string.format('hp : %s', c:hp()))
    print(string.format('damage : %s', c:damage()))
end