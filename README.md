# Spot Me 

This repo contains all the source code for the Spot Me project.

Please note that currently the master branch has code that isn't complete and isn't in a perfect working order, it just contains the farthest stage of development.


# Setup

<ol>
  <li>Install SQL Server Express, selection 'Basic' in installation type.</li>
  <li>Install SQL Server Management Studio 17.</li>
  <li>Launch SQL Server Management Studio 17 and click Cancel on the Connect window.</li>
  <li>View -> Registered Servers. On the Registered Servers Panel expand Database Enginer.</li>
  <li>Right Click Local Server Groups -> Tasks -> Registered Local Servers</li>
  <li>In Object Explorer click 'Connect', select your database engine in the 'Server Name' dropdown</li>
  <li>Right Click Databases in the Object Explorer -> New Database...</li>
  <li>Name new database SpotMeDB</li>
  <li>Right click SpotMeDb -> New Query</li>
  <li>Run SpotMeDBSetupSQL.sql to generate the tables and populate data.</li>
</ol>
