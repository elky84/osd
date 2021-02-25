function func(me)
	local map = me:map()
	local mobs = me:nears(1)

	print(string.format('mob count : %s', #mobs))

	for _, mob in pairs(mobs) do
		local base_hp = mob:base_hp()
		local damage = mob:hp() * 0.5

		if mob:hp() - damage < base_hp * 0.1 then
			damage = base_hp
		end

		mob:hp_add(-damage)
	end
end