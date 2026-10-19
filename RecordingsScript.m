% Välj din CSV-fil.
[file, folder] = uigetfile('*.csv', 'Välj pitchlogg');

if isequal(file, 0)
    return;
end

data = readtable(fullfile(folder, file));

figure;

plot(data.timeSeconds, data.detectedMidi, ...
    '.', 'Color', [0.65 0.65 0.65]);
hold on;

plot(data.timeSeconds, data.smoothedMidi, ...
    'b-', 'LineWidth', 1.5);

hold on;
stairs(data.timeSeconds, data.targetMidi, ...
    'r', 'LineWidth', 2.5);

xlabel('Låttid (s)');
ylabel('Tonhöjd (MIDI)');
legend('Rå ton', 'Medianfiltrerad ton', 'Målton');
grid on;