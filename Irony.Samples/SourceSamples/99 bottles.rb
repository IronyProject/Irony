#!/usr/bin/env ruby
# 99 bottles problem in Ruby

class Wall

  def initialize(num_of_bottles)
    @bottles = num_of_bottles
  end

  def sing_1_verse
    @output = sing_num(@bottles) + " on the wall, " + sing_num(@bottles) + "\n"
    @output += "take one down, pass it around, " + sing_num(@bottles-1) + "\n\n"
    return @output
  end

  def sing_all
    @output = ''
    while @bottles > 0 do
      @output += sing_1_verse()
      @bottles -= 1
    end
    return @output
  end
  
  def sing_num(num)
    @counter = (num > 1) ? 'bottles' : 'bottle'
    "#{num} #{@counter} of beer"
  end

end # class Wall

wall = Wall.new(99)
puts wall.sing_all()
