require 'fileutils'

lines = fields = 0
major = minor = rev = build = 0

open("version.h") {|file|
  while l = file.gets
    x = l.split(' ')
    if ( x[1] == "VERSION_MAJOR" )
      major = x[2].to_i
    elsif ( x[1] == "VERSION_MINOR" )
      minor = x[2].to_i
    elsif ( x[1] == "VERSION_REVISION" )
      rev = x[2].to_i
    elsif ( x[1] == "VERSION_BUILD" )
      build = x[2].to_i
    end
  end
}

build = build + 1

FileUtils.rm('version.tmp') if File.exists?('version.tmp')
FileUtils.mv('version.h', 'version.tmp')
vf = open("version.h", "w")


open("version.tmp") {|file|
  while l = file.gets
    if( l.split(' ')[1] == "VERSION_BUILD" )
      vf.puts "#define VERSION_BUILD      #{build}"
    elsif( l.split(' ')[1] == "VERSION_STRING" )
      vf.puts "#define VERSION_STRING     \"#{major}, #{minor}, #{rev}, #{build}\""
    elsif( l.split(' ')[1] == "VERSION_MESSAGE" )
      vf.puts "#define VERSION_MESSAGE    \"VER #{major}.#{minor}.#{rev}.#{build}(DBG-ONLY)\""
    else
      vf.puts l
    end
  end
}



